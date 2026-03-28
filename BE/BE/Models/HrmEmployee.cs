using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmEmployee
{
    public int EmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public int? TitleId { get; set; }

    public string? EmpCode { get; set; }

    public string? FullName { get; set; }

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
