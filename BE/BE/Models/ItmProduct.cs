using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmProduct
{
    public int ProductId { get; set; }

    public int? SubCatId { get; set; }

    public int? BrandId { get; set; }

    public int? UoMgroupId { get; set; }

    public int? TaxId { get; set; }

    public string? Sku { get; set; }
    public string? PackSize { get; set; }
    public decimal? Weight { get; set; }

    public string? ProductName { get; set; }

    public virtual ItmBrand? Brand { get; set; }

    public virtual ICollection<ItmImage> ItmImages { get; set; } = new List<ItmImage>();

    public virtual ICollection<ItmVariant> ItmVariants { get; set; } = new List<ItmVariant>();

    public virtual ItmSubCategory? SubCat { get; set; }

    public virtual FinTaxRate? Tax { get; set; }

    public virtual ItmUoMgroup? UoMgroup { get; set; }
}
