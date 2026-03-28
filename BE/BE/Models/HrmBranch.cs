using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmBranch
{
    public int BranchId { get; set; }

    public int? DistrictId { get; set; }

    public string? BranchCode { get; set; }

    public string? BranchName { get; set; }

    public virtual GeoDistrict? District { get; set; }

    public virtual ICollection<HrmDepartment> HrmDepartments { get; set; } = new List<HrmDepartment>();

    public virtual ICollection<WmsWarehouse> WmsWarehouses { get; set; } = new List<WmsWarehouse>();
}
