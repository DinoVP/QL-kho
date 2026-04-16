using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tên_Project_Của_Sếp.Models;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public ProductsController(QLKhoContext context) { _context = context; }

        private async Task<int> GetOrCreateUnitAsync(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName)) unitName = "SL";
            var unit = await _context.ItmUnits.FirstOrDefaultAsync(u => u.UnitName == unitName);
            if (unit == null)
            {
                unit = new ItmUnit
                {
                    UnitCode = "UNI_" + DateTime.Now.Ticks.ToString().Substring(8, 6),
                    UnitName = unitName
                };
                _context.ItmUnits.Add(unit);
                await _context.SaveChangesAsync();
            }
            return unit.UnitId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSkus()
        {
            var variants = await _context.ItmVariants
                .Include(v => v.Product).ThenInclude(p => p.Brand)
                .Include(v => v.Product).ThenInclude(p => p.SubCat).ThenInclude(sc => sc.Category)
                .Include(v => v.Product).ThenInclude(p => p.ItmProductUnits).ThenInclude(pu => pu.FromUnit)
                .Include(v => v.Product).ThenInclude(p => p.ItmProductUnits).ThenInclude(pu => pu.ToUnit)
                .ToListAsync();

            var result = variants.Select(v => {
                var productUnits = v.Product?.ItmProductUnits ?? new List<ItmProductUnit>();

                var productConvs = productUnits.Select(pu => new ConversionDto
                {
                    AltUnit = pu.FromUnit?.UnitName ?? "N/A",
                    Rate = pu.ToQty
                }).ToList();

                string unitName = v.Product?.PackSize ?? "SL";

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
                    Unit = unitName,
                    PackSize = displayPackSize,
                    ConversionRate = displayPackSize,
                    Conversions = productConvs,
                    Weight = v.Product?.Weight ?? 0,
                    Status = "active",
                    ImportPrice = v.BasePrice ?? 0 // ĐÃ FIX: Lấy giá từ DB lên
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
                    SubCatId = subCat?.SubCatId ?? 1,
                    PackSize = req.Unit,
                    Weight = req.Weight
                };

                _context.ItmProducts.Add(product);
                await _context.SaveChangesAsync();

                if (req.Conversions != null && req.Conversions.Any())
                {
                    int baseUnitId = await GetOrCreateUnitAsync(req.Unit);

                    foreach (var c in req.Conversions)
                    {
                        int altUnitId = await GetOrCreateUnitAsync(c.AltUnit);

                        _context.ItmProductUnits.Add(new ItmProductUnit
                        {
                            ProductId = product.ProductId,
                            FromUnitId = altUnitId,
                            FromQty = 1,
                            ToUnitId = baseUnitId,
                            ToQty = c.Rate
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                var variant = new ItmVariant
                {
                    ProductId = product.ProductId,
                    VariantSku = autoCode,
                    BasePrice = req.ImportPrice
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

                // ==========================================================
                // 🚀 BẮT MẠCH BIẾN ĐỘNG GIÁ TỰ ĐỘNG
                // ==========================================================
                if (variant.BasePrice != req.ImportPrice)
                {
                    var history = new ItmPriceHistory
                    {
                        VariantId = variant.VariantId,
                        OldPrice = variant.BasePrice ?? 0,
                        NewPrice = req.ImportPrice,
                        EffectiveDate = DateTime.Now,
                        UpdatedBy = "Admin", // Hiện tại để mặc định, sau này nối thông tin User vào
                        Source = "Cập nhật thủ công (Danh mục)"
                    };
                    _context.ItmPriceHistories.Add(history);
                }

                // Ghi đè giá mới vào sản phẩm
                variant.BasePrice = req.ImportPrice;

                if (variant.Product != null)
                {
                    variant.Product.ProductName = req.Name;

                    var brand = await _context.ItmBrands.FirstOrDefaultAsync(b => b.BrandName == req.Brand);
                    var cat = await _context.ItmCategories.FirstOrDefaultAsync(c => c.CatName == req.Category);

                    variant.Product.BrandId = brand?.BrandId;
                    variant.Product.PackSize = req.Unit;
                    variant.Product.Weight = req.Weight;

                    if (cat != null)
                    {
                        var subCat = await _context.ItmSubCategories.FirstOrDefaultAsync(s => s.CategoryId == cat.CategoryId);
                        if (subCat != null) variant.Product.SubCatId = subCat.SubCatId;
                    }

                    var oldConvs = await _context.ItmProductUnits.Where(p => p.ProductId == variant.Product.ProductId).ToListAsync();
                    if (oldConvs.Any())
                    {
                        _context.ItmProductUnits.RemoveRange(oldConvs);
                        await _context.SaveChangesAsync();
                    }

                    if (req.Conversions != null && req.Conversions.Any())
                    {
                        int baseUnitId = await GetOrCreateUnitAsync(req.Unit);

                        foreach (var c in req.Conversions)
                        {
                            int altUnitId = await GetOrCreateUnitAsync(c.AltUnit);

                            _context.ItmProductUnits.Add(new ItmProductUnit
                            {
                                ProductId = variant.Product.ProductId,
                                FromUnitId = altUnitId,
                                FromQty = 1,
                                ToUnitId = baseUnitId,
                                ToQty = c.Rate
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
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
                var variant = await _context.ItmVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.VariantId == id);
                if (variant == null) return NotFound(new { message = "Không tìm thấy Sản phẩm!" });

                bool hasStock = await _context.WmsStockBalances.AnyAsync(s => s.VariantId == id);
                if (hasStock) return BadRequest(new { message = "Sản phẩm này đã được xếp lên kệ, không thể xóa!" });

                if (variant.Product != null)
                {
                    var productUnits = await _context.ItmProductUnits.Where(pu => pu.ProductId == variant.Product.ProductId).ToListAsync();
                    if (productUnits.Any()) _context.ItmProductUnits.RemoveRange(productUnits);
                }

                _context.ItmVariants.Remove(variant);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa thành công!" });
            }
            catch (Exception) { return BadRequest(new { message = "Sản phẩm này đã phát sinh giao dịch, không thể xóa!" }); }
        }

        // ==========================================================
        // 🚀 API TRẢ VỀ LỊCH SỬ GIÁ CHO VUE.JS
        // ==========================================================
        [HttpGet("{id}/price-history")]
        public async Task<IActionResult> GetPriceHistory(int id)
        {
            var history = await _context.ItmPriceHistories
                .Where(h => h.VariantId == id)
                .OrderByDescending(h => h.EffectiveDate)
                .Select(h => new {
                    historyId = h.HistoryId,
                    oldPrice = h.OldPrice,
                    newPrice = h.NewPrice,
                    effectiveDate = h.EffectiveDate,
                    updatedBy = h.UpdatedBy,
                    source = h.Source
                })
                .ToListAsync();

            return Ok(history);
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
        public decimal ImportPrice { get; set; }
    }

    public class ConversionDto
    {
        public string AltUnit { get; set; }
        public decimal Rate { get; set; }
    }
}