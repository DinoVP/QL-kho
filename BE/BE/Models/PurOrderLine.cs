using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurOrderLine
{
    public int PolineId { get; set; }

    public int? Poid { get; set; }

    public int? VariantId { get; set; }

    public int? OrderQty { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual PurOrder? Po { get; set; }

    public virtual ICollection<PurReceiptLine> PurReceiptLines { get; set; } = new List<PurReceiptLine>();

    public virtual ItmVariant? Variant { get; set; }
}
