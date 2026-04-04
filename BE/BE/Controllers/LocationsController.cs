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
    public class LocationsController : ControllerBase
    {
        private readonly QLKhoContext _context;

        // LƯU TRỌNG TẢI TỐI ĐA TẠM VÀO RAM (Tránh phải sửa Database)
        private static readonly Dictionary<int, decimal> _locationMaxWeights = new Dictionary<int, decimal>();

        public LocationsController(QLKhoContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            var stocks = await _context.WmsStockBalances.ToListAsync();

            var locs = await _context.WmsLocations
                .Include(l => l.Rack).ThenInclude(r => r.Zone).ThenInclude(z => z.Warehouse)
                .Select(l => new LocationDto
                {
                    Id = l.LocationId,
                    Code = l.LocationCode,
                    // Bổ sung lấy WarehouseId để Vue lọc dữ liệu theo kho
                    WarehouseId = l.Rack != null && l.Rack.Zone != null ? l.Rack.Zone.WarehouseId : null,
                    Warehouse = l.Rack != null && l.Rack.Zone != null && l.Rack.Zone.Warehouse != null ? l.Rack.Zone.Warehouse.Whname : "Kho N/A",
                    Zone = l.Rack != null && l.Rack.Zone != null ? l.Rack.Zone.ZoneCode : "Dãy N/A",
                    Rack = l.Rack != null ? l.Rack.RackCode : "Kệ N/A",
                    Tier = ExtractTier(l.LocationCode),
                    Bin = ExtractBin(l.LocationCode),
                    Type = "Tiêu chuẩn"
                })
                .OrderByDescending(l => l.Id)
                .ToListAsync();

            foreach (var l in locs)
            {
                var locStocks = stocks.Where(s => s.LocationId == l.Id).ToList();
                l.CurrentQty = locStocks.Count;
                l.Status = locStocks.Count > 0 ? "full" : "empty";
                l.VariantIds = locStocks.Where(s => s.VariantId.HasValue).Select(s => s.VariantId.Value).ToList();

                // Mặc định kệ chịu được 500kg nếu sếp chưa thiết lập
                l.MaxWeight = _locationMaxWeights.ContainsKey(l.Id ?? 0) ? _locationMaxWeights[l.Id ?? 0] : 500m;
            }

            return Ok(locs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] LocationDto req)
        {
            try
            {
                var rack = await _context.WmsRacks.FirstOrDefaultAsync(r => r.RackCode == req.Rack);
                if (rack == null) return BadRequest(new { message = "Không tìm thấy Kệ này!" });

                var loc = new WmsLocation { LocationCode = req.Code?.ToUpper(), RackId = rack.RackId };
                _context.WmsLocations.Add(loc);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Thành công!", id = loc.LocationId });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationDto req)
        {
            try
            {
                var loc = await _context.WmsLocations.FindAsync(id);
                if (loc == null) return NotFound();

                loc.LocationCode = req.Code?.ToUpper();

                // Lưu cập nhật Trọng tải tối đa
                _locationMaxWeights[id] = req.MaxWeight;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var loc = await _context.WmsLocations.FindAsync(id);
                if (loc == null) return NotFound();
                if (await _context.WmsStockBalances.AnyAsync(s => s.LocationId == id))
                    return BadRequest(new { message = "Vị trí đang chứa hàng, không thể xóa!" });

                _context.WmsLocations.Remove(loc);
                await _context.SaveChangesAsync();
                _locationMaxWeights.Remove(id);
                return Ok(new { message = "Xóa thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ==========================================================
        // API MỚI: TÍNH NĂNG CẤT HÀNG VÀO KỆ (TỪ SƠ ĐỒ KHO)
        // ==========================================================
        [HttpPost("assign-stock")]
        public async Task<IActionResult> AssignStockToMap([FromBody] AssignStockDto req)
        {
            try
            {
                var location = await _context.WmsLocations.FirstOrDefaultAsync(l => l.LocationCode == req.LocationCode);
                if (location == null) return NotFound(new { message = "Không tìm thấy Vị trí kệ này!" });

                // Thêm 1 dòng tồn kho ảo vào vị trí này để vẽ lên sơ đồ
                var newStock = new WmsStockBalance
                {
                    VariantId = req.VariantId,
                    LocationId = location.LocationId,
                    Quantity = 1, // Số lượng tượng trưng
                    // Bổ sung lấy ID kho nếu cần: WarehouseId = location.Rack?.Zone?.WarehouseId
                };

                _context.WmsStockBalances.Add(newStock);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cất hàng vào ô thành công!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message }); }
        }

        // ==========================================================
        // API MỚI: TÍNH NĂNG LẤY HÀNG RA KHỎI KỆ (TỪ SƠ ĐỒ KHO)
        // Đã sửa để khớp với lỗi 404 sếp chụp trên Console
        // ==========================================================
        [HttpDelete("remove-stock-item/{locationCode}/{variantId}")]
        public async Task<IActionResult> RemoveStockItemFromMap(string locationCode, int variantId)
        {
            try
            {
                var location = await _context.WmsLocations.FirstOrDefaultAsync(l => l.LocationCode == locationCode);
                if (location == null) return NotFound(new { message = "Không tìm thấy Vị trí kệ này!" });

                var stockToRemove = await _context.WmsStockBalances
                    .FirstOrDefaultAsync(s => s.LocationId == location.LocationId && s.VariantId == variantId);

                if (stockToRemove == null) return BadRequest(new { message = "Không tìm thấy sản phẩm này trên kệ!" });

                _context.WmsStockBalances.Remove(stockToRemove);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã lấy hàng ra khỏi ô thành công!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message }); }
        }

        private static int ExtractTier(string code)
        {
            int tIndex = code?.IndexOf("-T") ?? -1;
            if (tIndex == -1) return 1;
            int oIndex = code.IndexOf("-O", tIndex);
            if (oIndex == -1) oIndex = code.Length;
            return int.TryParse(code.Substring(tIndex + 2, oIndex - tIndex - 2), out int t) ? t : 1;
        }

        private static int ExtractBin(string code)
        {
            int oIndex = code?.IndexOf("-O") ?? -1;
            if (oIndex == -1) return 1;
            return int.TryParse(code.Substring(oIndex + 2), out int o) ? o : 1;
        }
    }

    public class LocationDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public int? WarehouseId { get; set; } // Field mới để Frontend lọc kho
        public string? Warehouse { get; set; }
        public string? Zone { get; set; }
        public string? Rack { get; set; }
        public int Tier { get; set; }
        public int Bin { get; set; }
        public string? Type { get; set; }
        public int CurrentQty { get; set; }
        public string? Status { get; set; }
        public List<int> VariantIds { get; set; } = new List<int>();
        public decimal MaxWeight { get; set; }
    }

    public class AssignStockDto
    {
        public string LocationCode { get; set; }
        public int VariantId { get; set; }
    }
}