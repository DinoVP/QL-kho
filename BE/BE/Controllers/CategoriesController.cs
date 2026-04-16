using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tên_Project_Của_Sếp.Models;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public CategoriesController(QLKhoContext context)
        {
            _context = context;
        }

        // =========================================================================
        // CAMERA AN NINH TỰ ĐỘNG BẮT ID TỪ TOKEN
        // =========================================================================
        private async Task WriteAuditLogAsync(string actionType, string tableName, string details = "")
        {
            try
            {
                int? currentUserId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("UserId")
                               ?? User.FindFirst("id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                var log = new SysAuditLog
                {
                    UserId = currentUserId,
                    ActionType = actionType,
                    TableName = tableName,
                    Details = details,
                    LogDate = DateTime.Now
                };

                _context.SysAuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== LỖI GHI LOG: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.ItmCategories.Select(c => new CategoryDto
            {
                Id = "CAT_" + c.CategoryId,
                Code = c.CatCode,
                Name = c.CatName,
                Type = "Nhóm sản phẩm",
                Desc = "",
                Status = "active"
            }).ToListAsync();

            var brands = await _context.ItmBrands.Select(b => new CategoryDto
            {
                Id = "BRD_" + b.BrandId,
                Code = b.BrandCode,
                Name = b.BrandName,
                Type = "Thương hiệu",
                Desc = "",
                Status = "active"
            }).ToListAsync();

            // ĐÃ SỬA: Đổi từ ItmUoMgroups sang ItmUnits
            var uoms = await _context.ItmUnits.Select(u => new CategoryDto
            {
                Id = "UNI_" + u.UnitId, // Đổi tiền tố thành UNI_ cho chuẩn
                Code = u.UnitCode,
                Name = u.UnitName,
                Type = "Đơn vị tính",
                Desc = "ĐVT Linh hoạt", // ItmUnits chỉ có Code và Name, không có BaseUoM nữa
                Status = "active"
            }).ToListAsync();

            var result = categories.Concat(brands).Concat(uoms).ToList();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto request)
        {
            try
            {
                string newId = "";
                string autoCode = request.Code ?? "";

                if (request.Type == "Thương hiệu")
                {
                    if (string.IsNullOrWhiteSpace(autoCode))
                    {
                        int count = await _context.ItmBrands.CountAsync();
                        autoCode = "BRD" + (count + 1).ToString("D3");
                    }

                    var brand = new ItmBrand { BrandCode = autoCode, BrandName = request.Name };
                    _context.ItmBrands.Add(brand);
                    await _context.SaveChangesAsync();
                    newId = "BRD_" + brand.BrandId;
                }
                else if (request.Type == "Đơn vị tính")
                {
                    // ĐÃ SỬA: Tạo mới vào bảng ItmUnits
                    if (string.IsNullOrWhiteSpace(autoCode))
                    {
                        autoCode = "UNI_" + DateTime.Now.Ticks.ToString().Substring(8, 6);
                    }

                    var unit = new ItmUnit { UnitCode = autoCode, UnitName = request.Name };
                    _context.ItmUnits.Add(unit);
                    await _context.SaveChangesAsync();
                    newId = "UNI_" + unit.UnitId;
                }
                else // Nhóm sản phẩm
                {
                    if (string.IsNullOrWhiteSpace(autoCode))
                    {
                        int count = await _context.ItmCategories.CountAsync();
                        autoCode = "CAT" + (count + 1).ToString("D3");
                    }

                    var cat = new ItmCategory { CatCode = autoCode, CatName = request.Name };
                    _context.ItmCategories.Add(cat);
                    await _context.SaveChangesAsync();
                    newId = "CAT_" + cat.CategoryId;
                }

                // 🟢 GHI LOG
                await WriteAuditLogAsync("INSERT", $"Danh mục: {request.Name}", $"Tạo mới danh mục [{request.Type}] mã {autoCode}.");

                return Ok(new { message = "Thêm danh mục thành công!", id = newId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi hệ thống (có thể trùng mã): " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] CategoryDto request)
        {
            try
            {
                if (id.StartsWith("BRD_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var brand = await _context.ItmBrands.FindAsync(realId);
                    if (brand == null) return NotFound();
                    if (!string.IsNullOrWhiteSpace(request.Code)) brand.BrandCode = request.Code;
                    brand.BrandName = request.Name;
                }
                // ĐÃ SỬA: Bắt tiền tố UNI_ thay vì UOM_
                else if (id.StartsWith("UNI_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var unit = await _context.ItmUnits.FindAsync(realId);
                    if (unit == null) return NotFound();
                    if (!string.IsNullOrWhiteSpace(request.Code)) unit.UnitCode = request.Code;
                    unit.UnitName = request.Name;
                }
                else // CAT_
                {
                    int realId = int.Parse(id.Substring(4));
                    var cat = await _context.ItmCategories.FindAsync(realId);
                    if (cat == null) return NotFound();
                    if (!string.IsNullOrWhiteSpace(request.Code)) cat.CatCode = request.Code;
                    cat.CatName = request.Name;
                }

                await _context.SaveChangesAsync();

                // 🟡 GHI LOG
                await WriteAuditLogAsync("UPDATE", $"Danh mục: {request.Name}", $"Cập nhật thông tin danh mục {request.Name}.");

                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                string catName = "Danh mục";

                if (id.StartsWith("BRD_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var brand = await _context.ItmBrands.FindAsync(realId);
                    if (brand == null) return NotFound();
                    catName = brand.BrandName;
                    _context.ItmBrands.Remove(brand);
                }
                // ĐÃ SỬA: Xóa từ bảng ItmUnits
                else if (id.StartsWith("UNI_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var unit = await _context.ItmUnits.FindAsync(realId);
                    if (unit == null) return NotFound();
                    catName = unit.UnitName;
                    _context.ItmUnits.Remove(unit);
                }
                else // CAT_
                {
                    int realId = int.Parse(id.Substring(4));
                    var cat = await _context.ItmCategories.FindAsync(realId);
                    if (cat == null) return NotFound();
                    catName = cat.CatName;
                    _context.ItmCategories.Remove(cat);
                }

                await _context.SaveChangesAsync();
                await WriteAuditLogAsync("DELETE", $"Danh mục: {catName}", $"Đã xóa danh mục {catName} khỏi hệ thống.");

                return Ok(new { message = "Xóa danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không thể xóa do vướng dữ liệu sản phẩm đang sử dụng danh mục này!" });
            }
        }
    }

    public class CategoryDto
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Desc { get; set; }
        public string? Status { get; set; }
    }
}