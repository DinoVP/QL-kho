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
        // 1. LẤY DANH SÁCH PO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetPOs()
        {
            var pos = await _context.PurOrders
                .Include(p => p.PurOrderLines)
                .OrderByDescending(p => p.Poid)
                .ToListAsync();

            var result = pos.Select(p => new PoDto
            {
                Id = p.Poid,
                Code = p.Pocode,
                SupplierId = p.SupplierId ?? 0,
                Status = p.Status ?? "draft",
                Date = "",
                ExpectedDate = "",
                Note = "",
                TotalQty = p.PurOrderLines.Sum(l => (decimal)(l.OrderQty ?? 0)),
                Items = p.PurOrderLines.Select(l => new PoItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    Qty = (decimal)(l.OrderQty ?? 0)
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // ==========================================================
        // 2. LẬP PO MỚI
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> CreatePO([FromBody] PoDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.PurOrders.MaxAsync(r => (int?)r.Poid) ?? 0;
                    finalCode = $"PO{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                var po = new PurOrder
                {
                    Pocode = finalCode,
                    SupplierId = req.SupplierId,
                    Status = "pending"
                };

                _context.PurOrders.Add(po);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.PurOrderLines.Add(new PurOrderLine
                        {
                            Poid = po.Poid,
                            VariantId = item.VariantId,
                            OrderQty = Convert.ToInt32(item.Qty),
                            UnitPrice = 0 // GIÁ BẰNG 0 THEO YÊU CẦU CỦA SẾP
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Lập Đơn Đặt Hàng thành công!", code = po.Pocode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ==========================================================
        // 3. SỬA PO (Chắc chắn 100%)
        // ==========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePO(int id, [FromBody] PoDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var po = await _context.PurOrders.Include(p => p.PurOrderLines).FirstOrDefaultAsync(p => p.Poid == id);
                if (po == null || (po.Status != "pending" && po.Status != "draft"))
                    return BadRequest(new { message = "Chỉ được sửa Đơn đang nháp hoặc chờ duyệt!" });

                po.SupplierId = req.SupplierId;

                // XÓA SẠCH CHI TIẾT CŨ BÊN TRONG, THAY BẰNG CHI TIẾT MỚI (CHUẨN CHỈ)
                _context.PurOrderLines.RemoveRange(po.PurOrderLines);
                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.PurOrderLines.Add(new PurOrderLine
                        {
                            Poid = po.Poid,
                            VariantId = item.VariantId,
                            OrderQty = Convert.ToInt32(item.Qty),
                            UnitPrice = 0 // GIÁ BẰNG 0
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Cập nhật PO thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================================
        // 4. XÓA PO (Thêm mới theo yêu cầu sếp)
        // ==========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePO(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var po = await _context.PurOrders.Include(p => p.PurOrderLines).FirstOrDefaultAsync(p => p.Poid == id);
                if (po == null) return NotFound(new { message = "Không tìm thấy phiếu!" });

                if (po.Status != "pending" && po.Status != "draft")
                    return BadRequest(new { message = "Chỉ được xóa phiếu đang Nháp hoặc Chờ duyệt!" });

                // Phải xóa các dòng con trước khi xóa phiếu cha
                _context.PurOrderLines.RemoveRange(po.PurOrderLines);
                _context.PurOrders.Remove(po);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đã xóa Đơn Đặt Hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi xóa dữ liệu: " + ex.Message });
            }
        }

        // ==========================================================
        // 5. CÁC HÀM DUYỆT VÀ HOÀN THÀNH
        // ==========================================================
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApprovePO(int id)
        {
            var po = await _context.PurOrders.FindAsync(id);
            if (po != null) { po.Status = "approved"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã duyệt!" });
        }

        [HttpPut("{id}/send")]
        public async Task<IActionResult> SendPO(int id)
        {
            var po = await _context.PurOrders.FindAsync(id);
            if (po != null) { po.Status = "sent"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã gửi Email cho NCC!" });
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompletePO(int id)
        {
            var po = await _context.PurOrders.FindAsync(id);
            if (po != null) { po.Status = "completed"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Hoàn thành Đơn đặt hàng!" });
        }
    }

    public class PoDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }
        public string? ExpectedDate { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalPrice { get; set; }
        public List<PoItemDto>? Items { get; set; } = new List<PoItemDto>();
    }

    public class PoItemDto
    {
        public int? VariantId { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Price { get; set; }
    }
}