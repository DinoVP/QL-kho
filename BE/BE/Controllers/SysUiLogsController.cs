using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysUiLogsController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public SysUiLogsController(QLKhoContext context)
        {
            _context = context;
        }

        // GET: api/SysUiLogs
        // Dùng để Frontend gọi API lấy danh sách log đổ ra bảng (có phân trang cơ bản)
        [HttpGet]
        public async Task<IActionResult> GetLogs(int page = 1, int limit = 20, string eventType = null)
        {
            var query = _context.SysUiLogs.AsQueryable();

            // Lọc theo loại sự kiện nếu có (ví dụ: chỉ lấy log CLICK)
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(x => x.EventType == eventType);
            }

            // Đếm tổng số dòng để FE làm phân trang
            var total = await query.CountAsync();

            // Lấy dữ liệu: Sắp xếp mới nhất lên đầu -> Bỏ qua các dòng trang trước -> Lấy số dòng của trang hiện tại
            var data = await query.OrderByDescending(x => x.LogDate)
                                  .Skip((page - 1) * limit)
                                  .Take(limit)
                                  .ToListAsync();

            return Ok(new
            {
                Total = total,
                Data = data
            });
        }

        // POST: api/SysUiLogs
        // Dùng để Frontend bắn API lưu log xuống Database mỗi khi user thao tác
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] SysUiLog log)
        {
            if (log == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Đảm bảo lấy thời gian thực tế của Server lúc lưu
            log.LogDate = DateTime.Now;

            _context.SysUiLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(log);
        }
    }
}