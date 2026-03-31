using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public BranchesController(QLKhoContext context) { _context = context; }

        [HttpGet("available-managers")]
        public async Task<IActionResult> GetAvailableManagers()
        {
            var sql = @"
                SELECT e.EmployeeId as ManagerId, e.FullName as ManagerName, e.EmpCode as EmpCode, ISNULL(e.Email, '') as Email, ISNULL(e.Phone, '') as Phone
                FROM HRM_Employees e
                INNER JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId
                INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId
                INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId
                WHERE r.RoleCode = 'gd_chi_nhanh' AND u.IsActive = 1
                  AND e.EmployeeId NOT IN (SELECT ManagerId FROM HRM_Branches WHERE ManagerId IS NOT NULL)";
            var managers = await _context.Database.SqlQueryRaw<ManagerDto>(sql).ToListAsync();
            return Ok(managers);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBranches()
        {
            var branches = await _context.HrmBranches
                .Include(b => b.Manager)
                .Include(b => b.WmsWarehouses)
                .Select(b => new BranchDto
                {
                    Id = b.BranchId,
                    Code = b.BranchCode,
                    Name = b.BranchName,
                    Address = b.Address,
                    Phone = b.Phone,
                    Email = b.Email,
                    Status = b.IsActive == true ? "active" : "inactive",
                    WarehouseCount = b.WmsWarehouses.Count,
                    ManagerId = b.ManagerId,
                    ManagerName = b.Manager != null ? b.Manager.FullName : "",

                    // TODO: Sau này nối bảng Tồn kho thì đếm ở đây
                    ItemCount = 0,
                    TotalQuantity = 0
                }).ToListAsync();
            return Ok(branches);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] BranchCreateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string prefix = "CN";
                var lastBranch = await _context.HrmBranches.Where(b => b.BranchCode != null && b.BranchCode.StartsWith(prefix)).OrderByDescending(b => b.BranchCode).FirstOrDefaultAsync();
                string newCode = prefix + "001";
                if (lastBranch != null && lastBranch.BranchCode.Length > prefix.Length)
                {
                    string numPart = lastBranch.BranchCode.Substring(prefix.Length);
                    if (int.TryParse(numPart, out int currentNum)) newCode = prefix + (currentNum + 1).ToString("D3");
                }

                var newBranch = new HrmBranch
                {
                    BranchCode = newCode,
                    BranchName = request.Name,
                    Address = request.Address,
                    Phone = request.Phone,
                    Email = request.Email,
                    ManagerId = request.ManagerId,
                    IsActive = request.Status == "active"
                };

                _context.HrmBranches.Add(newBranch);
                await _context.SaveChangesAsync();

                if (request.ManagerId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET BranchId = {0} WHERE EmployeeId = {1}", newBranch.BranchId, request.ManagerId.Value);
                }

                await transaction.CommitAsync();
                return Ok(new { message = $"Đã tạo chi nhánh {newCode} thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] BranchCreateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _context.HrmBranches.FindAsync(id);
                if (branch == null) return NotFound(new { message = "Không tìm thấy chi nhánh này!" });

                branch.BranchName = request.Name;
                branch.Address = request.Address;
                branch.Phone = request.Phone;
                branch.Email = request.Email;
                branch.ManagerId = request.ManagerId;
                branch.IsActive = request.Status == "active";

                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET BranchId = NULL WHERE BranchId = {0} AND EmployeeId IN (SELECT u.EmployeeId FROM SYS_Users u INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE r.RoleCode = 'gd_chi_nhanh')", id);

                if (request.ManagerId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET BranchId = {0} WHERE EmployeeId = {1}", id, request.ManagerId.Value);
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Cập nhật chi nhánh thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =========================================================================
        // ĐÃ FIX: HÀM XÓA CHI NHÁNH - Xử lý luôn cả Phòng ban đang tồn tại
        // =========================================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _context.HrmBranches.FindAsync(id);
                if (branch == null) return NotFound(new { message = "Không tìm thấy chi nhánh!" });

                var hasWarehouses = await _context.WmsWarehouses.AnyAsync(w => w.BranchId == id);
                if (hasWarehouses) return BadRequest(new { message = "Sếp vui lòng xóa hết Kho trực thuộc trước khi xóa Chi nhánh nhé!" });

                // 1. Tháo BranchId của toàn bộ nhân viên
                await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET BranchId = NULL WHERE BranchId = {0}", id);

                // 2. Xóa sạch Phòng Ban (Departments) thuộc Chi nhánh này để không vướng khóa ngoại
                try
                {
                    // Tháo liên kết phòng ban của nhân viên trước
                    await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET DepartmentId = NULL WHERE DepartmentId IN (SELECT DepartmentId FROM HRM_Departments WHERE BranchId = {0})", id);
                    // Xóa phòng ban đi vì sếp chốt không dùng
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM HRM_Departments WHERE BranchId = {0}", id);
                }
                catch
                {
                    // Bỏ qua nếu bảng không tồn tại hoặc lỗi
                }

                // 3. Xóa chi nhánh
                _context.HrmBranches.Remove(branch);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Xóa chi nhánh thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Không thể xóa do vướng dữ liệu liên kết: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpGet("{branchId}/warehouses-detail")]
        public async Task<IActionResult> GetBranchWarehousesDetail(int branchId)
        {
            // Tạm thời gán ItemCount = 0, TotalQuantity = 0 chờ module Tồn kho
            var sql = @"
                SELECT w.WarehouseId, ISNULL(w.Whname, 'Chưa đặt tên') AS Whname,
                    ISNULL(w.WhAddress, 'Chưa có địa chỉ') AS WhAddress,
                    ISNULL((SELECT TOP 1 e.FullName FROM HRM_Employees e INNER JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE e.WarehouseId = w.WarehouseId AND r.RoleCode = 'ql_kho'), 'Chưa có QL') AS ManagerName,
                    (SELECT COUNT(*) FROM HRM_Employees e INNER JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE e.WarehouseId = w.WarehouseId AND r.RoleCode = 'nv_kho') AS EmployeeCount,
                    CAST(CASE WHEN EXISTS (SELECT 1 FROM HRM_Employees e INNER JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId WHERE e.WarehouseId = w.WarehouseId AND r.RoleCode = 'ql_kho') THEN 1 ELSE 0 END AS BIT) AS HasManager,
                    0 AS ItemCount, 
                    0 AS TotalQuantity
                FROM WMS_Warehouses w WHERE w.BranchId = {0}";
            try
            {
                var warehouses = await _context.Database.SqlQueryRaw<WarehouseDetailDto>(sql, branchId).ToListAsync();
                return Ok(warehouses);
            }
            catch (Exception ex) { return BadRequest(new { message = "Lỗi SQL: " + ex.Message }); }
        }

        [HttpPost("{branchId}/warehouses")]
        public async Task<IActionResult> CreateWarehouseQuick(int branchId, [FromBody] WarehouseCreateQuickDto request)
        {
            try
            {
                var branch = await _context.HrmBranches.FindAsync(branchId);
                if (branch == null) return NotFound();
                var newWh = new WmsWarehouse
                {
                    BranchId = branchId,
                    Whcode = "WH" + DateTime.Now.ToString("ssmmHH"),
                    Whname = request.Name,
                    WhAddress = request.Address
                };
                _context.WmsWarehouses.Add(newWh);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Tạo Kho thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { message = "Lỗi: " + ex.Message }); }
        }

        [HttpDelete("warehouses/{warehouseId}")]
        public async Task<IActionResult> DeleteWarehouse(int warehouseId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wh = await _context.WmsWarehouses.FindAsync(warehouseId);
                if (wh == null) return NotFound(new { message = "Không tìm thấy kho!" });

                await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET WarehouseId = NULL WHERE WarehouseId = {0}", warehouseId);

                _context.WmsWarehouses.Remove(wh);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Xóa kho thành công! Nhân sự thuộc kho đã được tự động gỡ." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("warehouses/{warehouseId}/clear-employees")]
        public async Task<IActionResult> ClearWarehouseEmployees(int warehouseId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("UPDATE HRM_Employees SET WarehouseId = NULL WHERE WarehouseId = {0}", warehouseId);
                return Ok(new { message = "Đã gỡ TẤT CẢ nhân sự ra khỏi kho!" });
            }
            catch (Exception ex) { return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message }); }
        }

        [HttpGet("warehouses/{warehouseId}/employees")]
        public async Task<IActionResult> GetWarehouseEmployees(int warehouseId)
        {
            var sql = @"
                SELECT e.EmployeeId, e.EmpCode, e.FullName, ISNULL(e.Phone, 'N/A') as Phone, r.RoleCode
                FROM HRM_Employees e
                INNER JOIN SYS_Users u ON e.EmployeeId = u.EmployeeId
                INNER JOIN SYS_UserRoles ur ON u.UserId = ur.UserId
                INNER JOIN SYS_Roles r ON ur.RoleId = r.RoleId
                WHERE e.WarehouseId = {0}";
            try
            {
                var emps = await _context.Database.SqlQueryRaw<WarehouseEmpDto>(sql, warehouseId).ToListAsync();
                return Ok(emps);
            }
            catch (Exception ex) { return BadRequest(new { message = "Lỗi: " + ex.Message }); }
        }
    }

    public class ManagerDto { public int ManagerId { get; set; } public string ManagerName { get; set; } public string EmpCode { get; set; } public string Email { get; set; } public string Phone { get; set; } }
    public class BranchDto { public int Id { get; set; } public string? Code { get; set; } public string? Name { get; set; } public string? Address { get; set; } public string? Phone { get; set; } public string? Email { get; set; } public string? Status { get; set; } public int WarehouseCount { get; set; } public int? ManagerId { get; set; } public string? ManagerName { get; set; } public int ItemCount { get; set; } public int TotalQuantity { get; set; } }
    public class BranchCreateDto { public string Name { get; set; } public string? Address { get; set; } public string? Phone { get; set; } public string? Email { get; set; } public int? ManagerId { get; set; } public string? Status { get; set; } }
    public class WarehouseDetailDto { public int WarehouseId { get; set; } public string Whname { get; set; } public string WhAddress { get; set; } public string ManagerName { get; set; } public int EmployeeCount { get; set; } public bool HasManager { get; set; } public int ItemCount { get; set; } public int TotalQuantity { get; set; } }
    public class WarehouseCreateQuickDto { public string Name { get; set; } public string Address { get; set; } }
    public class WarehouseEmpDto { public int EmployeeId { get; set; } public string EmpCode { get; set; } public string FullName { get; set; } public string Phone { get; set; } public string RoleCode { get; set; } }
}