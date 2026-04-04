using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public ProductsController(QLKhoContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAllSkus()
        {
            var variants = await _context.ItmVariants
                .Include(v => v.Product).ThenInclude(p => p.Brand)
                .Include(v => v.Product).ThenInclude(p => p.UoMgroup)
                .Include(v => v.Product).ThenInclude(p => p.SubCat).ThenInclude(sc => sc.Category)
                .ToListAsync();

            var result = variants.Select(v => new ProductSkuDto
            {
                Id = v.VariantId,
                Sku = v.VariantSku,
                Name = v.Product?.ProductName ?? "Sản phẩm ẩn",
                Category = v.Product?.SubCat?.Category?.CatName ?? "Chưa phân loại",
                Brand = v.Product?.Brand?.BrandName ?? "N/A",
                Unit = v.Product?.UoMgroup?.GroupName ?? "Thùng",

                // Sếp vẫn giữ PackSize cũ để không lỗi các chỗ khác
                PackSize = v.Product?.PackSize ?? "",
                // THÊM DÒNG NÀY: Trả về thêm tên ConversionRate để Frontend Stock.vue nhận diện được ngay!
                ConversionRate = v.Product?.PackSize ?? "1",

                Weight = v.Product?.Weight ?? 0,
                Status = "active"
            }).ToList();

            return Ok(result);
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

                string autoCode = "SP" + DateTime.Now.ToString("ddHHmmss");

                var product = new ItmProduct
                {
                    Sku = autoCode,
                    ProductName = req.Name,
                    BrandId = brand?.BrandId,
                    UoMgroupId = uom?.UoMgroupId,
                    SubCatId = subCat?.SubCatId ?? 1,
                    PackSize = req.PackSize, // Lưu thẳng Quy cách của sếp
                    Weight = req.Weight      // Lưu thẳng Cân nặng của sếp
                };

                _context.ItmProducts.Add(product);
                await _context.SaveChangesAsync();

                var variant = new ItmVariant
                {
                    ProductId = product.ProductId,
                    VariantSku = autoCode
                };
                _context.ItmVariants.Add(variant);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Thêm thành công!", id = variant.VariantId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi SQL: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSku(int id, [FromBody] ProductSkuDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var variant = await _context.ItmVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.VariantId == id);
                if (variant == null) return NotFound(new { message = "Không tìm thấy Sản phẩm!" });

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
                        if (subCat != null) variant.Product.SubCatId = subCat.SubCatId;
                    }

                    // Cập nhật lại Quy cách và Cân nặng
                    variant.Product.PackSize = req.PackSize;
                    variant.Product.Weight = req.Weight;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
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
            try
            {
                var variant = await _context.ItmVariants.FindAsync(id);
                if (variant == null) return NotFound(new { message = "Không tìm thấy Sản phẩm!" });

                bool hasStock = await _context.WmsStockBalances.AnyAsync(s => s.VariantId == id);
                if (hasStock) return BadRequest(new { message = "Sản phẩm này đã được xếp lên kệ, không thể xóa!" });

                _context.ItmVariants.Remove(variant);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa thành công!" });
            }
            catch (Exception) { return BadRequest(new { message = "Sản phẩm này đã phát sinh giao dịch, không thể xóa!" }); }
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
        public string? PackSize { get; set; }

        // THÊM BIẾN NÀY ĐỂ FRONTEND NHẬN DIỆN ĐƯỢC QUY CÁCH
        public string? ConversionRate { get; set; }

        public decimal Weight { get; set; }
        public string? Status { get; set; }
    }
}