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
    public class PoController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public PoController(QLKhoContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. LẤY DANH SÁCH (HỖ TRỢ CẢ PR VÀ PO TRÊN 1 CONTROLLER)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            // Sếp lưu ý: _context.PurOrders là tên biến trong QLKhoContext trỏ đến bảng PUR_Orders
            var orders = await _context.PurOrders
                .Include(p => p.PurOrderLines)
                .OrderByDescending(p => p.Poid)
                .ToListAsync();

            var result = orders.Select(p => new PoDto
            {
                Id = p.Poid,
                Code = p.Pocode,
                Type = p.Type ?? "PR",
                SupplierId = p.SupplierId ?? 0,
                Status = p.Status ?? "pending",
                Date = p.OrderDate?.ToString("yyyy-MM-dd") ?? "",
                Note = p.Note ?? "",
                // Tính tổng tiền cho PO (PR thì đơn giá = 0 nên tổng = 0)
                TotalAmount = p.PurOrderLines.Sum(l => (l.OrderQty ?? 0) * (l.UnitPrice ?? 0)),
                Items = p.PurOrderLines.Select(l => new PoItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    Qty = l.OrderQty ?? 0,
                    Price = l.UnitPrice ?? 0
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // ==========================================================
        // 2. TẠO MỚI PHIẾU (DÙNG CHO CẢ NÚT "XIN MUA" VÀ "LÊN PO")
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] PoDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int count = await _context.PurOrders.CountAsync(o => o.Type == req.Type) + 1;
                    string prefix = req.Type == "PO" ? "PO" : "PR";
                    finalCode = $"{prefix}{count.ToString().PadLeft(4, '0')}";
                }

                var order = new PurOrder
                {
                    Pocode = finalCode,
                    Type = req.Type ?? "PR",
                    SupplierId = (req.Type == "PO" && req.SupplierId > 0) ? req.SupplierId : null,
                    OrderDate = DateTime.Now,
                    Note = req.Note,
                    Status = req.Status ?? "pending"
                };

                _context.PurOrders.Add(order);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.PurOrderLines.Add(new PurOrderLine
                        {
                            Poid = order.Poid,
                            VariantId = item.VariantId,
                            OrderQty = (int)item.Qty,
                            UnitPrice = item.Price // Giá nhập
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Lưu thành công!", code = order.Pocode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================================
        // 3. CẬP NHẬT (DÙNG KHI CHUYỂN PR SANG PO HOẶC SỬA PHIẾU)
        // ==========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] PoDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.PurOrders.Include(p => p.PurOrderLines).FirstOrDefaultAsync(p => p.Poid == id);
                if (order == null) return NotFound();

                order.Type = req.Type ?? order.Type;
                order.SupplierId = req.SupplierId > 0 ? req.SupplierId : order.SupplierId;
                order.Note = req.Note ?? order.Note;
                order.Status = req.Status ?? order.Status;

                // Nếu có ngày từ phía FE gửi lên thì cập nhật, không thì giữ nguyên
                if (!string.IsNullOrEmpty(req.Date))
                    order.OrderDate = DateTime.Parse(req.Date);

                // Xóa chi tiết cũ, nạp lại chi tiết mới
                _context.PurOrderLines.RemoveRange(order.PurOrderLines);
                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.PurOrderLines.Add(new PurOrderLine
                        {
                            Poid = order.Poid,
                            VariantId = item.VariantId,
                            OrderQty = (int)item.Qty,
                            UnitPrice = item.Price
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================================
        // 4. DUYỆT NHANH TRẠNG THÁI
        // ==========================================================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var order = await _context.PurOrders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật trạng thái!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.PurOrders.Include(p => p.PurOrderLines).FirstOrDefaultAsync(p => p.Poid == id);
            if (order == null) return NotFound();

            _context.PurOrderLines.RemoveRange(order.PurOrderLines);
            _context.PurOrders.Remove(order);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa phiếu!" });
        }
    }

    // ==========================================================
    // DTO ĐỂ TRAO ĐỔI DỮ LIỆU VỚI FRONTEND
    // ==========================================================
    public class PoDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; } // "PR" hoặc "PO"
        public string? Date { get; set; }
        public int SupplierId { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PoItemDto>? Items { get; set; }
    }

    public class PoItemDto
    {
        public int VariantId { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}