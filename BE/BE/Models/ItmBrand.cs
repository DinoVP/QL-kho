using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmBrand
{
    public int BrandId { get; set; }

    public string? BrandCode { get; set; }

    public string? BrandName { get; set; }

    public virtual ICollection<ItmProduct> ItmProducts { get; set; } = new List<ItmProduct>();
}
