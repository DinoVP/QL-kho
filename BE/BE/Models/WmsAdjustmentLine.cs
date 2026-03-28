using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsAdjustmentLine
{
    public int AdjLineId { get; set; }

    public int? AdjId { get; set; }

    public int? VariantId { get; set; }

    public int? AdjQty { get; set; }

    public virtual WmsAdjustment? Adj { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
