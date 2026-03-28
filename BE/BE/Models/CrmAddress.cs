using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmAddress
{
    public int AddressId { get; set; }

    public int? PartnerId { get; set; }

    public int? DistrictId { get; set; }

    public string? AddressLine { get; set; }

    public virtual GeoDistrict? District { get; set; }

    public virtual CrmPartner? Partner { get; set; }
}
