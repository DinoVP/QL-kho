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

        // API Lấy danh sách Tồn kho (Chỉ lấy những hàng có Số lượng > 0)
        [HttpGet]
        public async Task<IActionResult> GetStock()
        {
            var stocks = await _context.WmsStockBalances
                .Where(s => s.Quantity > 0)
                .Select(s => new {
                    id = s.StockId,
                    variantId = s.VariantId,
                    locationId = s.LocationId,
                    qty = s.Quantity,
                    nsx = s.Nsx != null ? s.Nsx.Value.ToString("yyyy-MM-dd") : "",
                    hsd = s.Hsd != null ? s.Hsd.Value.ToString("yyyy-MM-dd") : ""
                })
                .ToListAsync();

            return Ok(stocks);
        }
    }
}