using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                Console.WriteLine("==== LỖI GHI LOG: " + ex.InnerException?.Message ?? ex.Message);
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
                Desc = "", // DB không có cột mô tả nên trả về rỗng
                Status = "active" // DB không có cột trạng thái nên fix cứng
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

            var uoms = await _context.ItmUoMgroups.Select(u => new CategoryDto
            {
                Id = "UOM_" + u.UoMgroupId,
                Code = "DVT",
                Name = u.GroupName,
                Type = "Đơn vị tính",
                Desc = u.BaseUoM, // Riêng UOM thì lấy BaseUoM làm mô tả
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
                    // Lấy ô Description trên FE làm Đơn vị cơ sở (BaseUoM)
                    var uom = new ItmUoMgroup { GroupName = request.Name, BaseUoM = request.Desc ?? "Cái" };
                    _context.ItmUoMgroups.Add(uom);
                    await _context.SaveChangesAsync();
                    newId = "UOM_" + uom.UoMgroupId;
                }
                else // Nhóm sản phẩm
                {
                    if (string.IsNullOrWhiteSpace(autoCode))
                    {
                        int count = await _context.ItmCategories.CountAsync();
                        autoCode = "CAT" + (count + 1).ToString("D3");
                    }

                    // Đã gỡ thuộc tính Description vì DB không có
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
                else if (id.StartsWith("UOM_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var uom = await _context.ItmUoMgroups.FindAsync(realId);
                    if (uom == null) return NotFound();
                    uom.GroupName = request.Name;
                    uom.BaseUoM = request.Desc ?? "Cái"; // Cập nhật BaseUoM
                }
                else // CAT_
                {
                    int realId = int.Parse(id.Substring(4));
                    var cat = await _context.ItmCategories.FindAsync(realId);
                    if (cat == null) return NotFound();
                    if (!string.IsNullOrWhiteSpace(request.Code)) cat.CatCode = request.Code;
                    cat.CatName = request.Name;
                    // Bỏ cập nhật Description vì DB không có
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
                else if (id.StartsWith("UOM_"))
                {
                    int realId = int.Parse(id.Substring(4));
                    var uom = await _context.ItmUoMgroups.FindAsync(realId);
                    if (uom == null) return NotFound();
                    catName = uom.GroupName;
                    _context.ItmUoMgroups.Remove(uom);
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