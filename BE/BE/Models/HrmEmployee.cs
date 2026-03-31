using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // THÊM DÒNG NÀY ĐỂ FIX LỖI

namespace BE.Models;

public partial class HrmEmployee
{
    public int EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? TitleId { get; set; }
    public string? EmpCode { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? BranchId { get; set; }
    public int? WarehouseId { get; set; }

    // --- CHỈ RÕ MỐI QUAN HỆ 2: NƠI LÀM VIỆC (CHI NHÁNH) ---
    [ForeignKey("BranchId")]
    [InverseProperty("HrmEmployees")]
    public virtual HrmBranch? Branch { get; set; }

    [ForeignKey("WarehouseId")]
    public virtual WmsWarehouse? Warehouse { get; set; }

    // --- CHỈ RÕ MỐI QUAN HỆ 1: CÁC CHI NHÁNH ĐANG LÀM QUẢN LÝ ---
    [InverseProperty("Manager")]
    public virtual ICollection<HrmBranch> ManagedBranches { get; set; } = new List<HrmBranch>();

    // ===============================================
    // CÁC THÔNG TIN CŨ GIỮ NGUYÊN
    // ===============================================
    public virtual ICollection<CrmSupplierEval> CrmSupplierEvals { get; set; } = new List<CrmSupplierEval>();
    public virtual HrmDepartment? Department { get; set; }
    public virtual ICollection<HrmPayroll> HrmPayrolls { get; set; } = new List<HrmPayroll>();
    public virtual ICollection<HrmTimesheet> HrmTimesheets { get; set; } = new List<HrmTimesheet>();
    public virtual ICollection<LogDriver> LogDrivers { get; set; } = new List<LogDriver>();
    public virtual ICollection<PurRequest> PurRequests { get; set; } = new List<PurRequest>();
    public virtual SysUser? SysUser { get; set; }
    public virtual HrmJobTitle? Title { get; set; }
    public virtual ICollection<WmsInvCheck> WmsInvChecks { get; set; } = new List<WmsInvCheck>();
}