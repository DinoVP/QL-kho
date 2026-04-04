using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public StockController(QLKhoContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. API LẤY DANH SÁCH TỒN KHO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetStock()
        {
            var stocks = await _context.WmsStockBalances
                .Include(s => s.Location)
                .ThenInclude(l => l.Rack)
                .ThenInclude(r => r.Zone)
                .Where(s => s.Quantity > 0)
                .Select(s => new {
                    id = s.StockId,
                    variantId = s.VariantId,
                    locationId = s.LocationId,
                    locationCode = s.Location != null ? s.Location.LocationCode : null,
                    qty = s.Quantity,
                    nsx = s.Nsx != null ? s.Nsx.Value.ToString("yyyy-MM-dd") : "",
                    hsd = s.Hsd != null ? s.Hsd.Value.ToString("yyyy-MM-dd") : "",

                    warehouseId = s.WarehouseId ?? (s.Location != null && s.Location.Rack != null && s.Location.Rack.Zone != null
                                  ? s.Location.Rack.Zone.WarehouseId
                                  : null)
                })
                .ToListAsync();

            return Ok(stocks);
        }

        // ==========================================================
        // 2. API CẤT HÀNG / CHUYỂN KỆ (PUTAWAY)
        // ==========================================================
        [HttpPost("move")]
        public async Task<IActionResult> MoveStock([FromBody] MoveStockDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // req.StockId giờ đã là kiểu long, FindAsync sẽ chạy mượt!
                var stock = await _context.WmsStockBalances.FindAsync(req.StockId);
                if (stock == null) return NotFound(new { message = "Không tìm thấy lô hàng này trong kho!" });

                int moveQty = (int)req.Qty; // Ép kiểu số thùng
                int currentQty = stock.Quantity ?? 0;

                if (moveQty <= 0 || moveQty > currentQty)
                    return BadRequest(new { message = "Số lượng cất không hợp lệ!" });

                var existingTargetStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                    s.VariantId == stock.VariantId &&
                    s.LocationId == req.ToLocationId &&
                    s.WarehouseId == stock.WarehouseId &&
                    s.Nsx == stock.Nsx &&
                    s.Hsd == stock.Hsd &&
                    s.StockId != stock.StockId);

                if (moveQty == currentQty)
                {
                    if (existingTargetStock != null)
                    {
                        existingTargetStock.Quantity = (existingTargetStock.Quantity ?? 0) + moveQty;
                        _context.WmsStockBalances.Remove(stock);
                    }
                    else
                    {
                        stock.LocationId = req.ToLocationId;
                    }
                }
                else
                {
                    stock.Quantity = currentQty - moveQty;

                    if (existingTargetStock != null)
                    {
                        existingTargetStock.Quantity = (existingTargetStock.Quantity ?? 0) + moveQty;
                    }
                    else
                    {
                        var newStock = new WmsStockBalance
                        {
                            VariantId = stock.VariantId,
                            LocationId = req.ToLocationId,
                            WarehouseId = stock.WarehouseId,
                            Quantity = moveQty,
                            Nsx = stock.Nsx,
                            Hsd = stock.Hsd
                        };
                        _context.WmsStockBalances.Add(newStock);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Luân chuyển hàng thành công!" });
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "LỖI SQL: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }

    public class MoveStockDto
    {
        // FIX LỖI "does not match the property type of long": Đổi int -> long
        public long StockId { get; set; }
        public int ToLocationId { get; set; }
        public decimal Qty { get; set; }
    }
}