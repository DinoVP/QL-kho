using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmPriceListDetail
{
    public int PriceListId { get; set; }

    public int VariantId { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual CrmPriceList PriceList { get; set; } = null!;

    public virtual ItmVariant Variant { get; set; } = null!;
}
