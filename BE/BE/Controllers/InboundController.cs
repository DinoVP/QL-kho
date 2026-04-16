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
        // 1. LẤY DANH SÁCH PHIẾU NHẬP (TÍCH HỢP AUTO-CLEAN)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetReceipts()
        {
            // TÍNH NĂNG MỚI: DỌN RÁC NGẦM (LAZY CLEANUP)
            // Mỗi lần mở trang, quét và xóa các phiếu "rejected" (Từ chối) quá 7 ngày
            try
            {
                var sevenDaysAgo = DateTime.Now.AddDays(-7);
                var expiredReceipts = await _context.PurReceipts
                    .Where(r => r.Status == "rejected" && r.ReceiptDate < sevenDaysAgo)
                    .ToListAsync();

                if (expiredReceipts.Any())
                {
                    var expiredIds = expiredReceipts.Select(r => r.Grnid).ToList();
                    var linesToDelete = await _context.PurReceiptLines.Where(l => expiredIds.Contains(l.Grnid ?? 0)).ToListAsync();

                    _context.PurReceiptLines.RemoveRange(linesToDelete);
                    _context.PurReceipts.RemoveRange(expiredReceipts);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi dọn rác ngầm: " + ex.Message); }

            // Lấy dữ liệu bình thường
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
        // 2. TẠO PHIẾU NHẬP MỚI
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] InboundReceiptDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                    WarehouseId = req.WarehouseId,
                    Status = "pending", // Vẫn giữ là pending ban đầu
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
                            LocationId = null,
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }
                await _context.SaveChangesAsync();

                // NẾU LÀ GIÁM ĐỐC TẠO -> DUYỆT LUÔN (Theo code Vue.js truyền status là approved)
                if (req.Status == "approved")
                {
                    receipt.Status = "approved";
                    await _context.SaveChangesAsync();
                }

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
        // 3. XÁC NHẬN NHẬP KHO VÀ 🚀 BẮT MẠCH GIÁ TỰ ĐỘNG
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

                // 1. GỘP CÁC SẢN PHẨM TRÙNG NHAU TRONG PHIẾU ĐỂ CỘNG TỒN KHO
                var groupedItems = receipt.PurReceiptLines
                    .GroupBy(l => l.VariantId)
                    .Select(g => new {
                        VariantId = g.Key,
                        Nsx = g.FirstOrDefault(x => x.Nsx != null)?.Nsx,
                        Hsd = g.FirstOrDefault(x => x.Hsd != null)?.Hsd,
                        TotalQty = g.Sum(l => l.ReceivedQty ?? 0),
                        LatestPrice = g.LastOrDefault()?.Price ?? 0 // Lấy giá của dòng cuối cùng làm mốc
                    }).ToList();

                foreach (var item in groupedItems)
                {
                    // --- 🚀 CODE BẮT MẠCH BIẾN ĐỘNG GIÁ TỰ ĐỘNG ---
                    var variant = await _context.ItmVariants.FindAsync(item.VariantId);
                    if (variant != null && variant.BasePrice != item.LatestPrice)
                    {
                        // Ghi log Lịch sử giá
                        var priceHistory = new ItmPriceHistory
                        {
                            VariantId = variant.VariantId,
                            OldPrice = variant.BasePrice ?? 0,
                            NewPrice = item.LatestPrice,
                            EffectiveDate = DateTime.Now,
                            UpdatedBy = "Auto (Warehouse)", // Hệ thống tự ghi nhận lúc nhập kho
                            Source = $"Tự động từ Phiếu Nhập kho số: {receipt.Grncode}"
                        };
                        _context.ItmPriceHistories.Add(priceHistory);

                        // Cập nhật giá mới cho sản phẩm
                        variant.BasePrice = item.LatestPrice;
                    }
                    // --- KẾT THÚC BẮT MẠCH GIÁ ---

                    // --- TIẾP TỤC CỘNG TỒN KHO NHƯ BÌNH THƯỜNG ---
                    var existingStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.LocationId == null &&
                        s.WarehouseId == safeWhId);

                    if (existingStock != null)
                    {
                        existingStock.Quantity = (existingStock.Quantity ?? 0) + item.TotalQty;
                        if (item.Nsx != null) existingStock.Nsx = item.Nsx;
                        if (item.Hsd != null) existingStock.Hsd = item.Hsd;
                        _context.WmsStockBalances.Update(existingStock);
                    }
                    else
                    {
                        var newStock = new WmsStockBalance
                        {
                            VariantId = item.VariantId,
                            LocationId = null,
                            WarehouseId = safeWhId,
                            Quantity = item.TotalQty,
                            Nsx = item.Nsx,
                            Hsd = item.Hsd
                        };
                        _context.WmsStockBalances.Add(newStock);
                    }
                }

                receipt.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Nhập hàng thành công! Giá vốn đã được cập nhật tự động." });
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