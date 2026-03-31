using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public EmployeesController(QLKhoContext context) { _context = context; }

        // =========================================================================
        // CAMERA AN NINH: DÙNG ĐỂ GHI LOG BÊN PHÂN HỆ NHÂN SỰ
        // =========================================================================
        private async Task WriteAuditLogAsync(string actionType, string tableName)
        {
            try
            {
                var log = new SysAuditLog
                {
                    UserId = null, // ĐÃ FIX: Tạm để null để né lỗi khóa ngoại (chờ làm xong module Login)
                    ActionType = actionType,
                    TableName = tableName,
                    LogDate = DateTime.Now
                };

                // ĐÃ FIX: Dùng hàm Add chung của _context để không sợ sai tên DbSet
                _context.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Đã mở khóa báo lỗi ra Console của Visual Studio để dễ debug
                Console.WriteLine("==== LỖI GHI LOG: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var sql = @"
                SELECT e.EmployeeId, e.EmpCode, e.FullName, e.Phone, ISNULL(e.Email, '') as Email,
                       b.BranchName as BranchName, w.Whname as WarehouseName,
                       u.Username, r.RoleCode, CAST(ISNULL(u.IsActive, 0) AS BIT) as IsActive,
                       e.BranchId, e.WarehouseId
                FROM HRM_Employees e
                LEFT JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId
                LEFT JOIN SYS_UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN SYS_Roles r ON ur.RoleId = r.RoleId
                LEFT JOIN HRM_Branches b ON e.BranchId = b.BranchId
                LEFT JOIN WMS_Warehouses w ON e.WarehouseId = w.WarehouseId";

            var query = await _context.Database.SqlQueryRaw<EmployeeDto>(sql).ToListAsync();
            return Ok(query);
        }

        [HttpGet("workplaces")]
        public async Task<IActionResult> GetWorkplaces()
        {
            var branches = await _context.HrmBranches.Select(b => new { b.BranchId, b.BranchName }).ToListAsync();
            var warehouses = await _context.WmsWarehouses.Select(w => new { w.WarehouseId, WarehouseName = w.Whname, w.BranchId }).ToListAsync();
            return Ok(new { branches, warehouses });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string prefix = request.RoleCode switch { "admin" => "ADM", "giam_doc" => "CEO", "gd_chi_nhanh" => "GDC", "ql_kho" => "QLK", "nv_kho" => "NVK", _ => "EMP" };
                var lastEmp = await _context.HrmEmployees.Where(e => e.EmpCode.StartsWith(prefix)).OrderByDescending(e => e.EmpCode).FirstOrDefaultAsync();
                string newCode = prefix + (lastEmp != null ? (int.Parse(lastEmp.EmpCode.Substring(prefix.Length)) + 1).ToString("D3") : "001");

                var newEmp = new HrmEmployee
                {
                    EmpCode = newCode,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    BranchId = request.BranchId,
                    WarehouseId = request.WarehouseId
                };
                _context.HrmEmployees.Add(newEmp);
                await _context.SaveChangesAsync();

                var newUser = new SysUser { EmployeeId = newEmp.EmployeeId, Username = request.Username, PasswordHash = request.Password, IsActive = true };
                _context.SysUsers.Add(newUser);
                await _context.SaveChangesAsync();

                var role = await _context.SysRoles.FirstOrDefaultAsync(r => r.RoleCode == request.RoleCode);
                if (role != null) await _context.Database.ExecuteSqlRawAsync("INSERT INTO SYS_UserRoles (UserID, RoleID) VALUES ({0}, {1})", newUser.UserId, role.RoleId);

                if (request.RoleCode == "gd_chi_nhanh" && request.BranchId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Branches SET ManagerId = {0} WHERE BranchId = {1}", newEmp.EmployeeId, request.BranchId.Value);
                }

                await transaction.CommitAsync();

                // 🟢 GHI LOG: THÊM MỚI NHÂN VIÊN
                await WriteAuditLogAsync("INSERT", $"Nhân sự: {newCode} - {request.FullName}");

                return Ok(new { message = $"Tạo thành công {newCode}" });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message }); }
        }

        [HttpPut("remove-workplace/{id}")]
        public async Task<IActionResult> RemoveWorkplace(int id)
        {
            var emp = await _context.HrmEmployees.FindAsync(id);
            if (emp == null) return NotFound();

            string empName = emp.FullName; // Lưu tên để ghi log

            var roleCode = await _context.Database.SqlQueryRaw<string>("SELECT r.RoleCode AS Value FROM SYS_Users u INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE u.EmployeeId = {0}", id).FirstOrDefaultAsync();

            if (roleCode == "gd_chi_nhanh" && emp.BranchId.HasValue)
            {
                await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Branches SET ManagerId = NULL WHERE BranchId = {0}", emp.BranchId.Value);
            }

            emp.BranchId = null;
            emp.WarehouseId = null;
            await _context.SaveChangesAsync();

            // 🟡 GHI LOG: GỠ VỊ TRÍ
            await WriteAuditLogAsync("UPDATE", $"Nhân sự: Gỡ vị trí làm việc của {empName}");

            return Ok(new { message = "Đã gỡ nhân viên khỏi vị trí làm việc" });
        }

        [HttpPut("{id}/assign-workplace")]
        public async Task<IActionResult> AssignWorkplace(int id, [FromBody] AssignWorkplaceDto request)
        {
            try
            {
                var emp = await _context.HrmEmployees.FindAsync(id);
                if (emp == null) return NotFound();

                emp.BranchId = request.BranchId;
                emp.WarehouseId = request.WarehouseId;
                await _context.SaveChangesAsync();

                var roleCode = await _context.Database.SqlQueryRaw<string>("SELECT r.RoleCode AS Value FROM SYS_Users u INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE u.EmployeeId = {0}", id).FirstOrDefaultAsync();

                if (roleCode == "gd_chi_nhanh" && request.BranchId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Branches SET ManagerId = {0} WHERE BranchId = {1}", id, request.BranchId.Value);
                }

                // 🟡 GHI LOG: GÁN VỊ TRÍ
                await WriteAuditLogAsync("UPDATE", $"Nhân sự: Gán vị trí mới cho {emp.FullName}");

                return Ok(new { message = "Cập nhật vị trí làm việc thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message }); }
        }

        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.SysUsers.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user == null) return NotFound();
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            var empName = await _context.HrmEmployees.Where(e => e.EmployeeId == id).Select(e => e.FullName).FirstOrDefaultAsync();

            // 🔴 GHI LOG: KHÓA / MỞ KHÓA TÀI KHOẢN
            string statusStr = user.IsActive == true ? "Mở khóa" : "Khóa";
            await WriteAuditLogAsync("UPDATE", $"Tài khoản: {statusStr} tài khoản của {empName}");

            return Ok(new { message = user.IsActive == true ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản" });
        }
    }

    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmpCode { get; set; }
        public string FullName { get; set; }
        public string? BranchName { get; set; }
        public string? WarehouseName { get; set; }
        public string Username { get; set; }
        public string RoleCode { get; set; }
        public bool IsActive { get; set; }
        public int? BranchId { get; set; }
        public int? WarehouseId { get; set; }
    }

    public class EmployeeCreateDto
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RoleCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? BranchId { get; set; }
        public int? WarehouseId { get; set; }
    }

    public class AssignWorkplaceDto
    {
        public int? BranchId { get; set; }
        public int? WarehouseId { get; set; }
    }
}