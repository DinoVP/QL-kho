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

            var uomGroupIds = variants.Where(v => v.Product?.UoMgroupId != null).Select(v => v.Product.UoMgroupId).Distinct().ToList();
            var allConversions = await _context.ItmUoMconversions.Where(c => uomGroupIds.Contains(c.UoMgroupId)).ToListAsync();

            var result = variants.Select(v => {
                var productConvs = allConversions.Where(c => c.UoMgroupId == v.Product?.UoMgroupId)
                                                 .Select(c => new ConversionDto { AltUnit = c.AltUoM, Rate = c.ConvRate ?? 1 })
                                                 .ToList();

                // XỬ LÝ ẨN MÃ UOM_SP ĐỂ HIỆN ĐÚNG TÊN ĐVT CƠ BẢN (VD: Lon, Cái)
                string unitName = v.Product?.UoMgroup?.GroupName ?? "SL";
                if (unitName.StartsWith("UOM_"))
                {
                    var parts = unitName.Split('_');
                    unitName = parts.Length > 2 ? parts[2] : "SL";
                }

                string displayPackSize = productConvs.Any()
                    ? string.Join(" | ", productConvs.Select(c => $"1 {c.AltUnit} = {c.Rate:0.##} {unitName}"))
                    : v.Product?.PackSize ?? "";

                return new ProductSkuDto
                {
                    Id = v.VariantId,
                    Sku = v.VariantSku,
                    Name = v.Product?.ProductName ?? "Sản phẩm ẩn",
                    Category = v.Product?.SubCat?.Category?.CatName ?? "Chưa phân loại",
                    Brand = v.Product?.Brand?.BrandName ?? "N/A",
                    Unit = unitName, // Đã fix hiện đúng ĐVT
                    PackSize = displayPackSize,
                    ConversionRate = displayPackSize,
                    Conversions = productConvs,
                    Weight = v.Product?.Weight ?? 0,
                    Status = "active"
                };
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
                int? finalUomGroupId = uom?.UoMgroupId;

                if (req.Conversions != null && req.Conversions.Any())
                {
                    // Lưu giấu tên ĐVT Cơ bản vào cuối chuỗi để lúc GET gọi ra
                    var customUom = new ItmUoMgroup { GroupName = "UOM_" + autoCode + "_" + req.Unit };
                    _context.ItmUoMgroups.Add(customUom);
                    await _context.SaveChangesAsync();
                    finalUomGroupId = customUom.UoMgroupId;

                    foreach (var c in req.Conversions)
                    {
                        _context.ItmUoMconversions.Add(new ItmUoMconversion
                        {
                            UoMgroupId = finalUomGroupId,
                            AltUoM = c.AltUnit,
                            ConvRate = c.Rate
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                var product = new ItmProduct
                {
                    Sku = autoCode,
                    ProductName = req.Name,
                    BrandId = brand?.BrandId,
                    UoMgroupId = finalUomGroupId,
                    SubCatId = subCat?.SubCatId ?? 1,
                    PackSize = req.PackSize,
                    Weight = req.Weight
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
                    int? finalUomGroupId = uom?.UoMgroupId;

                    if (cat != null)
                    {
                        var subCat = await _context.ItmSubCategories.FirstOrDefaultAsync(s => s.CategoryId == cat.CategoryId);
                        if (subCat != null) variant.Product.SubCatId = subCat.SubCatId;
                    }

                    if (req.Conversions != null && req.Conversions.Any())
                    {
                        var currentUom = await _context.ItmUoMgroups.FindAsync(variant.Product.UoMgroupId);
                        if (currentUom == null || !currentUom.GroupName.StartsWith("UOM_"))
                        {
                            var customUom = new ItmUoMgroup { GroupName = "UOM_" + variant.Product.Sku + "_" + req.Unit };
                            _context.ItmUoMgroups.Add(customUom);
                            await _context.SaveChangesAsync();
                            finalUomGroupId = customUom.UoMgroupId;
                        }
                        else
                        {
                            // Cập nhật lại Tên ĐVT cơ bản lỡ user có đổi
                            currentUom.GroupName = "UOM_" + variant.Product.Sku + "_" + req.Unit;
                            finalUomGroupId = currentUom.UoMgroupId;
                            var oldConvs = await _context.ItmUoMconversions.Where(c => c.UoMgroupId == finalUomGroupId).ToListAsync();
                            _context.ItmUoMconversions.RemoveRange(oldConvs);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var c in req.Conversions)
                        {
                            _context.ItmUoMconversions.Add(new ItmUoMconversion
                            {
                                UoMgroupId = finalUomGroupId,
                                AltUoM = c.AltUnit,
                                ConvRate = c.Rate
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    variant.Product.UoMgroupId = finalUomGroupId;
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
        public string? ConversionRate { get; set; }
        public List<ConversionDto> Conversions { get; set; } = new List<ConversionDto>();
        public decimal Weight { get; set; }
        public string? Status { get; set; }
    }

    public class ConversionDto
    {
        public string AltUnit { get; set; }
        public decimal Rate { get; set; }
    }
}