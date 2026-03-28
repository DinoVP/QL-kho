using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class GeoDistrict
{
    public int DistrictId { get; set; }

    public int? ProvinceId { get; set; }

    public string? DistrictName { get; set; }

    public virtual ICollection<CrmAddress> CrmAddresses { get; set; } = new List<CrmAddress>();

    public virtual ICollection<HrmBranch> HrmBranches { get; set; } = new List<HrmBranch>();

    public virtual GeoProvince? Province { get; set; }
}
