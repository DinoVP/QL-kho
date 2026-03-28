using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmDepartment
{
    public int DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public virtual HrmBranch? Branch { get; set; }

    public virtual ICollection<HrmEmployee> HrmEmployees { get; set; } = new List<HrmEmployee>();
}
