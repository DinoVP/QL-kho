using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public InboundController(QLKhoContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. LẤY DANH SÁCH PHIẾU NHẬP
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetReceipts()
        {
            var receipts = await _context.PurReceipts
                .Include(r => r.PurReceiptLines)
                .OrderByDescending(r => r.Grnid)
                .ToListAsync();

            var result = receipts.Select(r => new InboundReceiptDto
            {
                Id = r.Grnid,
                Code = r.Grncode,
                Date = r.ReceiptDate?.ToString("yyyy-MM-dd") ?? "",
                SupplierId = r.SupplierId ?? 0,
                // Trả về WarehouseId để Frontend (Vue) thực hiện lọc dữ liệu đa kho
                WarehouseId = r.WarehouseId,
                Note = r.Note ?? "",
                Status = r.Status ?? "pending",
                TotalQty = r.PurReceiptLines.Sum(l => l.ReceivedQty ?? 0m),
                TotalPrice = r.PurReceiptLines.Sum(l => (l.ReceivedQty ?? 0m) * (l.Price ?? 0m)),
                Items = r.PurReceiptLines.Select(l => new InboundItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    Qty = l.ReceivedQty ?? 0m,
                    Price = l.Price ?? 0m,
                    LocationId = l.LocationId,
                    Nsx = l.Nsx?.ToString("yyyy-MM-dd"),
                    Hsd = l.Hsd?.ToString("yyyy-MM-dd")
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // ==========================================================
        // 2. TẠO PHIẾU NHẬP MỚI (CHỜ DUYỆT)
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] InboundReceiptDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tự sinh mã phiếu PN0001, PN0002... nếu frontend không gửi
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.PurReceipts.MaxAsync(r => (int?)r.Grnid) ?? 0;
                    finalCode = $"PN{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                var receipt = new PurReceipt
                {
                    Grncode = finalCode,
                    ReceiptDate = DateTime.TryParse(req.Date, out var d) ? d : DateTime.Now,
                    SupplierId = req.SupplierId,
                    WarehouseId = req.WarehouseId, // Lưu ID kho của người tạo
                    Status = "pending",
                    Note = req.Note
                };
                _context.PurReceipts.Add(receipt);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.PurReceiptLines.Add(new PurReceiptLine
                        {
                            Grnid = receipt.Grnid,
                            VariantId = item.VariantId,
                            ReceivedQty = Convert.ToInt32(item.Qty ?? 0),
                            Price = item.Price,
                            LocationId = null, // Hàng mới lập phiếu chưa có kệ
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Lập phiếu thành công!", code = receipt.Grncode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ==========================================================
        // 3. XÁC NHẬN NHẬP KHO (KẾT THÚC PHIẾU)
        // Hàm này sẽ đẩy hàng vào bãi tập kết (LocationId = NULL)
        // ==========================================================
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteReceipt(int id)
        {
            var receipt = await _context.PurReceipts.Include(r => r.PurReceiptLines).FirstOrDefaultAsync(r => r.Grnid == id);
            if (receipt == null || (receipt.Status != "approved" && receipt.Status != "pending"))
                return BadRequest(new { message = "Phiếu không ở trạng thái hợp lệ để nhập kho!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int safeWhId = receipt.WarehouseId ?? 1;

                foreach (var item in receipt.PurReceiptLines)
                {
                    // FIX LỖI UNIQUE KEY: Kiểm tra xem sản phẩm cùng NSX/HSD đã có ở bãi chờ chưa
                    var existingStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.LocationId == null &&  // Khu chờ nhập
                        s.WarehouseId == safeWhId &&
                        s.Nsx == item.Nsx &&
                        s.Hsd == item.Hsd);

                    if (existingStock != null)
                    {
                        // Đã có -> Cộng dồn số lượng
                        existingStock.Quantity = (existingStock.Quantity ?? 0) + (item.ReceivedQty ?? 0);
                        _context.WmsStockBalances.Update(existingStock);
                    }
                    else
                    {
                        // Chưa có -> Tạo dòng tồn kho mới tại bãi tập kết (LocationId = null)
                        var newStock = new WmsStockBalance
                        {
                            VariantId = item.VariantId,
                            LocationId = null,
                            WarehouseId = safeWhId,
                            Quantity = item.ReceivedQty ?? 0,
                            Nsx = item.Nsx,
                            Hsd = item.Hsd
                        };
                        _context.WmsStockBalances.Add(newStock);
                    }
                }

                receipt.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Nhập hàng thành công! Hàng đã vào bãi tập kết." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "LỖI SQL: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ==========================================================
        // CÁC HÀM DUYỆT / TỪ CHỐI / XÓA
        // ==========================================================
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveReceipt(int id)
        {
            var receipt = await _context.PurReceipts.FindAsync(id);
            if (receipt != null) { receipt.Status = "approved"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã duyệt phiếu!" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectReceipt(int id)
        {
            var receipt = await _context.PurReceipts.FindAsync(id);
            if (receipt != null) { receipt.Status = "rejected"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã từ chối phiếu!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var receipt = await _context.PurReceipts.Include(r => r.PurReceiptLines).FirstOrDefaultAsync(r => r.Grnid == id);
            if (receipt == null || receipt.Status != "pending") return BadRequest(new { message = "Chỉ được xóa phiếu chờ duyệt!" });

            _context.PurReceiptLines.RemoveRange(receipt.PurReceiptLines);
            _context.PurReceipts.Remove(receipt);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
    }

    // ==========================================================
    // DTO MODELS
    // ==========================================================
    public class InboundReceiptDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }
        public int? SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalPrice { get; set; }
        public List<InboundItemDto>? Items { get; set; } = new List<InboundItemDto>();
    }

    public class InboundItemDto
    {
        public int? VariantId { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Price { get; set; }
        public int? LocationId { get; set; }
        public string? Nsx { get; set; }
        public string? Hsd { get; set; }
    }
}