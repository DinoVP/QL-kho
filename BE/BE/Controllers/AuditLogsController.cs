using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public AuditLogsController(QLKhoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            try
            {
                // BƯỚC 1: Kéo dữ liệu thô từ Database lên trước (ĐÃ BỔ SUNG CỘT DETAILS)
                var rawLogs = await (from a in _context.Set<SysAuditLog>()
                                     join u in _context.SysUsers on a.UserId equals u.UserId into uGroup
                                     from u in uGroup.DefaultIfEmpty()
                                     join e in _context.HrmEmployees on u.EmployeeId equals e.EmployeeId into eGroup
                                     from e in eGroup.DefaultIfEmpty()
                                     orderby a.LogDate descending
                                     select new
                                     {
                                         LogId = a.LogId,
                                         UserId = a.UserId,
                                         FullName = e != null ? e.FullName : null,
                                         Username = u != null ? u.Username : null,
                                         ActionType = a.ActionType,
                                         TableName = a.TableName,
                                         Details = a.Details, // <--- THÊM DÒNG NÀY ĐỂ MÓC CHI TIẾT LÊN
                                         LogDate = a.LogDate
                                     }).Take(500).ToListAsync(); // Lấy 500 dòng mới nhất

                // BƯỚC 2: Xử lý gán chữ Tiếng Việt trên RAM của C#
                var logs = rawLogs.Select(a => new AuditLogDto
                {
                    LogId = a.LogId,
                    UserId = a.UserId,
                    // Ưu tiên: Tên nhân viên -> Tên tài khoản -> Nếu null hết thì hiện "Hệ thống"
                    UserName = !string.IsNullOrWhiteSpace(a.FullName) ? a.FullName
                             : (!string.IsNullOrWhiteSpace(a.Username) ? a.Username : "Hệ thống"),
                    ActionType = a.ActionType ?? "SYSTEM",
                    TableName = a.TableName ?? "Hệ thống",
                    Details = a.Details, // <--- CHUYỂN DỮ LIỆU VÀO DTO
                    LogDate = a.LogDate
                }).ToList();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi tải nhật ký: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }

    // ==========================================
    // DTO ĐỂ GỬI DỮ LIỆU LÊN VUE
    // ==========================================
    public class AuditLogDto
    {
        public long LogId { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public string Details { get; set; } // <--- KHAI BÁO THÊM TRƯỜNG DETAILS Ở ĐÂY
        public DateTime? LogDate { get; set; }
    }
}