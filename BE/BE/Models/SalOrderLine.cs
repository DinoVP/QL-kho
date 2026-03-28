using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalOrderLine
{
    public int SolineId { get; set; }

    public int? Soid { get; set; }

    public int? VariantId { get; set; }

    public int? OrderQty { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<SalDeliveryLine> SalDeliveryLines { get; set; } = new List<SalDeliveryLine>();

    public virtual SalOrder? So { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
