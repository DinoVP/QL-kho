using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class GeoCountry
{
    public int CountryId { get; set; }

    public string? CountryCode { get; set; }

    public string? CountryName { get; set; }

    public virtual ICollection<GeoProvince> GeoProvinces { get; set; } = new List<GeoProvince>();
}
