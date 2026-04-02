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
    public class ProductsController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public ProductsController(QLKhoContext context) { _context = context; }

        // =========================================================================
        // CAMERA AN NINH TỰ ĐỘNG BẮT ID TỪ TOKEN
        // =========================================================================
        private async Task WriteAuditLogAsync(string actionType, string tableName, string details = "")
        {
            try
            {
                int? currentUserId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("UserId") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId)) currentUserId = parsedId;

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
            catch (Exception ex) { Console.WriteLine("==== LỖI GHI LOG: " + ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSkus()
        {
            // SỬA LỖI CS0103: Dùng đường dẫn trực tiếp (Navigation) thay vì JOIN thủ công
            var query = await _context.ItmVariants
                .Select(v => new ProductSkuDto
                {
                    Id = v.VariantId,
                    Sku = v.VariantSku,
                    Name = v.Product.ProductName,
                    Category = v.Product.SubCat != null && v.Product.SubCat.Category != null ? v.Product.SubCat.Category.CatName : "Chưa phân loại",
                    Brand = v.Product.Brand != null ? v.Product.Brand.BrandName : "N/A",
                    Unit = v.Product.UoMgroup != null ? v.Product.UoMgroup.GroupName : "Cái",
                    MinStock = 10,
                    MaxStock = 100,
                    Price = v.BasePrice ?? 0,
                    Desc = "", // SỬA LỖI CS0117: DB Không có cột mô tả, trả về rỗng cho an toàn
                    Status = "active"
                }).ToListAsync();

            return Ok(query);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSku([FromBody] ProductSkuDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var brand = await _context.ItmBrands.FirstOrDefaultAsync(b => b.BrandName == req.Brand);
                var cat = await _context.ItmCategories.FirstOrDefaultAsync(c => c.CatName == req.Category);
                var uom = await _context.ItmUoMgroups.FirstOrDefaultAsync(u => u.GroupName == req.Unit);

                var subCat = await _context.ItmSubCategories.FirstOrDefaultAsync(s => s.CategoryId == (cat != null ? cat.CategoryId : 1));
                if (subCat == null)
                {
                    subCat = new ItmSubCategory { CategoryId = cat?.CategoryId ?? 1, SubCatName = "Mặc định" };
                    _context.ItmSubCategories.Add(subCat);
                    await _context.SaveChangesAsync();
                }

                // Tạo Sản phẩm gốc
                var product = new ItmProduct
                {
                    ProductName = req.Name,
                    BrandId = brand?.BrandId,
                    UoMgroupId = uom?.UoMgroupId,
                    SubCatId = subCat.SubCatId
                    // SỬA LỖI CS0117: Đã gỡ bỏ Description ở đây
                };
                _context.ItmProducts.Add(product);
                await _context.SaveChangesAsync();

                // Tạo Biến thể SKU
                var variant = new ItmVariant
                {
                    ProductId = product.ProductId,
                    VariantSku = string.IsNullOrWhiteSpace(req.Sku) ? "SKU-" + DateTime.Now.Ticks.ToString().Substring(10) : req.Sku,
                    BasePrice = req.Price
                };
                _context.ItmVariants.Add(variant);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await WriteAuditLogAsync("INSERT", $"Sản phẩm: {req.Name}", $"Tạo SKU mới: {variant.VariantSku}");
                return Ok(new { message = "Thêm thành công!", id = variant.VariantId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ===============================================
        // VIẾT THÊM HÀM SỬA CHO SẾP CẬP NHẬT GIAO DIỆN
        // ===============================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSku(int id, [FromBody] ProductSkuDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var variant = await _context.ItmVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.VariantId == id);
                if (variant == null) return NotFound(new { message = "Không tìm thấy SKU này!" });

                // Cập nhật giá & SKU
                variant.BasePrice = req.Price;
                if (!string.IsNullOrWhiteSpace(req.Sku)) variant.VariantSku = req.Sku;

                // Cập nhật Product gốc
                if (variant.Product != null)
                {
                    variant.Product.ProductName = req.Name;

                    var brand = await _context.ItmBrands.FirstOrDefaultAsync(b => b.BrandName == req.Brand);
                    var cat = await _context.ItmCategories.FirstOrDefaultAsync(c => c.CatName == req.Category);
                    var uom = await _context.ItmUoMgroups.FirstOrDefaultAsync(u => u.GroupName == req.Unit);

                    variant.Product.BrandId = brand?.BrandId;
                    variant.Product.UoMgroupId = uom?.UoMgroupId;

                    if (cat != null)
                    {
                        var subCat = await _context.ItmSubCategories.FirstOrDefaultAsync(s => s.CategoryId == cat.CategoryId);
                        if (subCat == null)
                        {
                            subCat = new ItmSubCategory { CategoryId = cat.CategoryId, SubCatName = "Mặc định" };
                            _context.ItmSubCategories.Add(subCat);
                            await _context.SaveChangesAsync();
                        }
                        variant.Product.SubCatId = subCat.SubCatId;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await WriteAuditLogAsync("UPDATE", $"Sản phẩm: {req.Name}", $"Cập nhật SKU: {variant.VariantSku}");
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi cập nhật: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSku(int id)
        {
            var variant = await _context.ItmVariants.FindAsync(id);
            if (variant == null) return NotFound(new { message = "Không tìm thấy SKU!" });

            string skuCode = variant.VariantSku;
            _context.ItmVariants.Remove(variant);
            await _context.SaveChangesAsync();

            await WriteAuditLogAsync("DELETE", $"SKU: {skuCode}", $"Đã xóa mã SKU khỏi hệ thống.");
            return Ok(new { message = "Đã xóa thành công!" });
        }
    }

    public class ProductSkuDto
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Unit { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public decimal Price { get; set; }
        public string? Desc { get; set; }
        public string? Status { get; set; }
    }
}