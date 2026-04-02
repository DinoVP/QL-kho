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
    public class WarehouseLayoutController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public WarehouseLayoutController(QLKhoContext context) { _context = context; }

        private async Task WriteAuditLogAsync(string actionType, string tableName, string details = "")
        {
            try
            {
                int? currentUserId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId)) currentUserId = parsedId;

                _context.SysAuditLogs.Add(new SysAuditLog
                {
                    UserId = currentUserId,
                    ActionType = actionType,
                    TableName = tableName,
                    Details = details,
                    LogDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception) { /* Bỏ qua lỗi log để không gián đoạn app */ }
        }

        [HttpGet("zones")]
        public async Task<IActionResult> GetZones()
        {
            var zones = await _context.WmsZones
                .Include(z => z.Warehouse)
                .Select(z => new ZoneDto
                {
                    Id = z.ZoneId,
                    Code = z.ZoneCode,
                    WarehouseId = z.WarehouseId,
                    WarehouseName = z.Warehouse != null ? z.Warehouse.Whname : "N/A",
                    RackCount = _context.WmsRacks.Count(r => r.ZoneId == z.ZoneId)
                }).ToListAsync();

            // YÊU CẦU CỦA SẾP: Dãy nào không có kệ thì TỰ ĐỘNG MẤT
            var emptyZones = zones.Where(z => z.RackCount == 0).ToList();
            if (emptyZones.Any())
            {
                foreach (var ez in emptyZones)
                {
                    var z = await _context.WmsZones.FindAsync(ez.Id);
                    if (z != null) _context.WmsZones.Remove(z);
                }
                await _context.SaveChangesAsync();
                zones = zones.Where(z => z.RackCount > 0).ToList(); // Lọc lại danh sách trả về
            }

            return Ok(zones);
        }

        [HttpPost("zones")]
        public async Task<IActionResult> CreateZone([FromBody] ZoneDto req)
        {
            var zone = new WmsZone { ZoneCode = req.Code?.ToUpper(), WarehouseId = req.WarehouseId ?? 1 };
            _context.WmsZones.Add(zone);
            await _context.SaveChangesAsync();
            return Ok(new { id = zone.ZoneId });
        }

        [HttpGet("racks")]
        public async Task<IActionResult> GetRacks()
        {
            var racks = await _context.WmsRacks
                .Include(r => r.Zone).ThenInclude(z => z.Warehouse)
                .Select(r => new RackDto
                {
                    Id = r.RackId,
                    Code = r.RackCode,
                    ZoneId = r.ZoneId,
                    ZoneCode = r.Zone != null ? r.Zone.ZoneCode : "N/A",
                    WarehouseName = r.Zone != null && r.Zone.Warehouse != null ? r.Zone.Warehouse.Whname : "N/A",
                    LocationCount = _context.WmsLocations.Count(l => l.RackId == r.RackId)
                }).ToListAsync();
            return Ok(racks);
        }

        [HttpPost("racks")]
        public async Task<IActionResult> CreateRack([FromBody] RackDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rack = new WmsRack { RackCode = req.Code?.ToUpper(), ZoneId = req.ZoneId ?? 1 };
                _context.WmsRacks.Add(rack);
                await _context.SaveChangesAsync();

                // Tự động sinh Ô/Vị trí (Location) dựa trên Tầng và Ô FE gửi lên
                int tiers = req.Tiers > 0 ? req.Tiers : 4;
                int bins = req.BinsPerTier > 0 ? req.BinsPerTier : 2;

                for (int t = tiers; t >= 1; t--)
                {
                    for (int b = 1; b <= bins; b++)
                    {
                        _context.WmsLocations.Add(new WmsLocation
                        {
                            RackId = rack.RackId,
                            LocationCode = $"{rack.RackCode}-T{t}-O{b}"
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await WriteAuditLogAsync("INSERT", $"Kệ hàng: {rack.RackCode}", "Xây kệ mới.");
                return Ok(new { message = "Thành công!", id = rack.RackId });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message }); }
        }

        // ĐÃ THÊM API CẬP NHẬT KỆ (PUT) CHO SẾP
        [HttpPut("racks/{id}")]
        public async Task<IActionResult> UpdateRack(int id, [FromBody] RackDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rack = await _context.WmsRacks.FindAsync(id);
                if (rack == null) return NotFound(new { message = "Không tìm thấy Kệ!" });

                rack.RackCode = req.Code?.ToUpper();

                if (req.Tiers > 0 && req.BinsPerTier > 0)
                {
                    // Đập đi xây lại ô vị trí
                    var oldLocs = _context.WmsLocations.Where(l => l.RackId == id);
                    _context.WmsLocations.RemoveRange(oldLocs);
                    await _context.SaveChangesAsync();

                    for (int t = req.Tiers; t >= 1; t--)
                    {
                        for (int b = 1; b <= req.BinsPerTier; b++)
                        {
                            _context.WmsLocations.Add(new WmsLocation
                            {
                                RackId = id,
                                LocationCode = $"{rack.RackCode}-T{t}-O{b}"
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await WriteAuditLogAsync("UPDATE", $"Kệ hàng: {rack.RackCode}", "Cập nhật thông số Kệ.");
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("racks/{id}")]
        public async Task<IActionResult> DeleteRack(int id)
        {
            var rack = await _context.WmsRacks.FindAsync(id);
            if (rack == null) return NotFound(new { message = "Không tìm thấy Kệ!" });

            int zoneId = rack.ZoneId ?? 0;
            string code = rack.RackCode;

            // Xóa Vị trí con trước khi xóa Kệ
            var locs = _context.WmsLocations.Where(l => l.RackId == id);
            _context.WmsLocations.RemoveRange(locs);
            _context.WmsRacks.Remove(rack);
            await _context.SaveChangesAsync();

            // YÊU CẦU: Dọn dẹp Dãy nếu vừa xóa cái kệ cuối cùng
            bool hasRacks = await _context.WmsRacks.AnyAsync(r => r.ZoneId == zoneId);
            if (!hasRacks)
            {
                var zone = await _context.WmsZones.FindAsync(zoneId);
                if (zone != null) _context.WmsZones.Remove(zone);
            }

            await _context.SaveChangesAsync();
            await WriteAuditLogAsync("DELETE", $"Kệ hàng: {code}", "Đập bỏ Kệ.");
            return Ok(new { message = "Xóa kệ thành công!" });
        }

        [HttpGet("warehouses-dropdown")]
        public async Task<IActionResult> GetWarehousesForDropdown()
        {
            return Ok(await _context.WmsWarehouses.Select(w => new { id = w.WarehouseId, name = w.Whname }).ToListAsync());
        }
    }

    public class ZoneDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int RackCount { get; set; }
    }

    public class RackDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneCode { get; set; }
        public string? WarehouseName { get; set; }
        public int LocationCount { get; set; }
        public int Tiers { get; set; }
        public int BinsPerTier { get; set; }
    }
}