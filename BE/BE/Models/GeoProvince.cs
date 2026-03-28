using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class GeoProvince
{
    public int ProvinceId { get; set; }

    public int? CountryId { get; set; }

    public string? ProvinceName { get; set; }

    public virtual GeoCountry? Country { get; set; }

    public virtual ICollection<GeoDistrict> GeoDistricts { get; set; } = new List<GeoDistrict>();
}
