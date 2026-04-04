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
    public class DefectController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public DefectController(QLKhoContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetDefects()
        {
            var defects = await _context.WmsDefects.Include(d => d.WmsDefectLines).OrderByDescending(d => d.DefectId).ToListAsync();
            var result = defects.Select(d => new DefectDto
            {
                Id = d.DefectId,
                Code = d.DefectCode ?? $"HL{d.DefectId.ToString().PadLeft(4, '0')}",
                Date = d.DefectDate?.ToString("yyyy-MM-dd") ?? "",
                Note = d.Note ?? "",
                Status = d.Status ?? "pending",
                Items = d.WmsDefectLines.Select(l => new DefectItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    LocationId = l.LocationId,
                    Qty = l.DefectQty ?? 0m, // Sửa thành DefectQty
                    Reason = l.Reason ?? "",
                    Nsx = l.Nsx?.ToString("yyyy-MM-dd"),
                    Hsd = l.Hsd?.ToString("yyyy-MM-dd")
                }).ToList()
            }).ToList();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDefect([FromBody] DefectDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.WmsDefects.MaxAsync(r => (int?)r.DefectId) ?? 0;
                    finalCode = $"HL{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                var defect = new WmsDefect
                {
                    DefectCode = finalCode,
                    DefectDate = DateTime.TryParse(req.Date, out var d) ? d : DateTime.Now,
                    Status = "pending",
                    Note = req.Note
                };
                _context.WmsDefects.Add(defect);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.WmsDefectLines.Add(new WmsDefectLine
                        {
                            DefectId = defect.DefectId,
                            VariantId = item.VariantId,
                            LocationId = item.LocationId,
                            DefectQty = Convert.ToInt32(item.Qty ?? 0), // Sửa thành DefectQty
                            Reason = item.Reason,
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Lập phiếu báo lỗi thành công!" });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteDefect(int id)
        {
            var defect = await _context.WmsDefects.Include(d => d.WmsDefectLines).FirstOrDefaultAsync(d => d.DefectId == id);
            if (defect == null || defect.Status == "completed") return BadRequest(new { message = "Phiếu không hợp lệ!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in defect.WmsDefectLines)
                {
                    var stock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId && s.LocationId == item.LocationId && s.Nsx == item.Nsx && s.Hsd == item.Hsd);

                    if (stock == null || stock.Quantity < item.DefectQty) // Sửa thành DefectQty
                        throw new Exception($"Không đủ tồn kho trên kệ để xuất hủy sản phẩm ID {item.VariantId}!");

                    stock.Quantity -= item.DefectQty; // Sửa thành DefectQty
                    if (stock.Quantity <= 0) _context.WmsStockBalances.Remove(stock);
                }
                defect.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Đã trừ hàng lỗi khỏi Tồn kho!" });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message }); }
        }
    }

    public class DefectDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public List<DefectItemDto>? Items { get; set; } = new List<DefectItemDto>();
    }

    public class DefectItemDto
    {
        public int? VariantId { get; set; }
        public int? LocationId { get; set; }
        public decimal? Qty { get; set; }
        public string? Reason { get; set; }
        public string? Nsx { get; set; }
        public string? Hsd { get; set; }
    }
}