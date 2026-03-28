using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models; // Gọi thư mục chứa 100 bảng của sếp vào

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly QLKhoContext _context;

        // Tiêm (Dependency Injection) Database Context vào Controller
        public WarehousesController(QLKhoContext context)
        {
            _context = context;
        }

        // 1. API LẤY DANH SÁCH KHO BÃI
        // Nút GET: api/Warehouses
        [HttpGet]
        public async Task<IActionResult> GetAllWarehouses()
        {
            // Lấy toàn bộ dữ liệu từ bảng WmsWarehouses
            var warehouses = await _context.WmsWarehouses.ToListAsync();

            // Trả về mã 200 (Thành công) kèm theo cục dữ liệu JSON
            return Ok(warehouses);
        }

        // 2. API TẠO MỚI MỘT KHO BÃI
        // Nút POST: api/Warehouses
        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody] WmsWarehouse newWarehouse)
        {
            if (newWarehouse == null)
            {
                return BadRequest("Dữ liệu kho không hợp lệ.");
            }

            // Thêm vào database
            _context.WmsWarehouses.Add(newWarehouse);
            await _context.SaveChangesAsync();

            // Trả về kho vừa tạo
            return Ok(newWarehouse);
        }
    }
}