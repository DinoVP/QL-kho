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
    public class InvCheckController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public InvCheckController(QLKhoContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. LẤY DANH SÁCH KIỂM KÊ
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetChecks()
        {
            var checks = await _context.WmsInvChecks
                .Include(c => c.WmsInvCheckLines)
                .Include(c => c.WmsAdjustments) // Lấy kèm phiếu điều chỉnh để biết đã chốt sổ chưa
                .OrderByDescending(c => c.CheckId)
                .ToListAsync();

            var result = checks.Select(c => new CheckDto
            {
                Id = c.CheckId,
                Code = c.CheckCode,
                WarehouseId = c.WarehouseId ?? 0,

                // MẸO: Nếu đã có phiếu Điều Chỉnh (Adjustment) nghĩa là ĐÃ CHỐT SỔ
                Status = c.WmsAdjustments.Any() ? "completed" : "draft",

                // Các cột này không có trong DB nên em để trống, giao diện Vue vẫn chạy mượt
                Date = "",
                Note = "",

                Items = c.WmsInvCheckLines.Select(l => new CheckItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    SystemQty = (decimal)(l.SystemQty ?? 0),
                    ActualQty = (decimal)(l.ActualQty ?? 0),
                    // Tự động tính chênh lệch ngay lúc lấy data
                    DiffQty = (decimal)((l.ActualQty ?? 0) - (l.SystemQty ?? 0)),
                    Reason = ""
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // ==========================================================
        // 2. LẬP / LƯU NHÁP PHIẾU KIỂM KÊ
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> CreateCheck([FromBody] CheckDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.WmsInvChecks.MaxAsync(r => (int?)r.CheckId) ?? 0;
                    finalCode = $"KK{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                // CHUẨN MODEL: Bỏ Date, Status, Note
                var check = new WmsInvCheck
                {
                    CheckCode = finalCode,
                    WarehouseId = req.WarehouseId
                };
                _context.WmsInvChecks.Add(check);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        // CHUẨN MODEL: Bỏ DiffQty, Reason
                        _context.WmsInvCheckLines.Add(new WmsInvCheckLine
                        {
                            CheckId = check.CheckId,
                            VariantId = item.VariantId,
                            SystemQty = Convert.ToInt32(item.SystemQty),
                            ActualQty = Convert.ToInt32(item.ActualQty)
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Lưu Phiếu Kiểm Kê thành công!", code = check.CheckCode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ==========================================================
        // 3. CẬP NHẬT PHIẾU KIỂM KÊ
        // ==========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCheck(int id, [FromBody] CheckDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var check = await _context.WmsInvChecks
                    .Include(c => c.WmsInvCheckLines)
                    .Include(c => c.WmsAdjustments)
                    .FirstOrDefaultAsync(c => c.CheckId == id);

                if (check == null) return NotFound(new { message = "Không tìm thấy phiếu!" });
                if (check.WmsAdjustments.Any()) return BadRequest(new { message = "Phiếu đã chốt sổ (Đã tạo phiếu điều chỉnh kho) nên không được sửa!" });

                check.WarehouseId = req.WarehouseId;

                _context.WmsInvCheckLines.RemoveRange(check.WmsInvCheckLines);
                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.WmsInvCheckLines.Add(new WmsInvCheckLine
                        {
                            CheckId = check.CheckId,
                            VariantId = item.VariantId,
                            SystemQty = Convert.ToInt32(item.SystemQty),
                            ActualQty = Convert.ToInt32(item.ActualQty)
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Cập nhật Phiếu Kiểm Kê thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================================
        // 4. CHỐT SỔ (CÂN BẰNG KHO THỰC TẾ)
        // ==========================================================
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteCheck(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var check = await _context.WmsInvChecks
                    .Include(c => c.WmsInvCheckLines)
                    .Include(c => c.WmsAdjustments)
                    .FirstOrDefaultAsync(c => c.CheckId == id);

                if (check == null) return NotFound(new { message = "Không tìm thấy phiếu!" });
                if (check.WmsAdjustments.Any()) return BadRequest(new { message = "Phiếu này đã được chốt sổ trước đó!" });

                int safeWhId = check.WarehouseId ?? 1;

                // 1. Duyệt qua từng mặt hàng kiểm đếm để bù/trừ kho
                foreach (var item in check.WmsInvCheckLines)
                {
                    int diff = (item.ActualQty ?? 0) - (item.SystemQty ?? 0);
                    if (diff == 0) continue; // Khớp thì bỏ qua

                    // Tìm vị trí của mặt hàng này trong bãi chờ (LocationId = null)
                    var stockToAdjust = await _context.WmsStockBalances
                        .FirstOrDefaultAsync(s => s.VariantId == item.VariantId && s.WarehouseId == safeWhId);

                    if (stockToAdjust != null)
                    {
                        // Cập nhật thẳng số lượng tồn kho
                        stockToAdjust.Quantity = (stockToAdjust.Quantity ?? 0) + diff;
                        if (stockToAdjust.Quantity < 0) stockToAdjust.Quantity = 0; // Chống âm kho
                        _context.WmsStockBalances.Update(stockToAdjust);
                    }
                    else if (diff > 0)
                    {
                        // Nếu lệch thừa mà chưa có dòng tồn kho nào, tạo mới nằm ở Bãi chờ nhập
                        var newStock = new WmsStockBalance
                        {
                            VariantId = item.VariantId,
                            WarehouseId = safeWhId,
                            LocationId = null,
                            Quantity = diff
                        };
                        _context.WmsStockBalances.Add(newStock);
                    }
                }

                // 2. SINH PHIẾU ĐIỀU CHỈNH ĐỂ ĐÁNH DẤU LÀ ĐÃ CHỐT SỔ
                var adjustment = new WmsAdjustment
                {
                    CheckId = check.CheckId,
                    AdjCode = $"ADJ-{check.CheckCode}"
                };
                _context.WmsAdjustments.Add(adjustment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đã chốt sổ và tự động cân bằng kho thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "LỖI SQL: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }

    // ==========================================================
    // DTO MODELS (Dùng để giao tiếp với Vue)
    // ==========================================================
    public class CheckDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }
        public int? WarehouseId { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public List<CheckItemDto>? Items { get; set; } = new List<CheckItemDto>();
    }

    public class CheckItemDto
    {
        public int? VariantId { get; set; }
        public decimal? SystemQty { get; set; }
        public decimal? ActualQty { get; set; }
        public decimal? DiffQty { get; set; }
        public string? Reason { get; set; }
    }
}