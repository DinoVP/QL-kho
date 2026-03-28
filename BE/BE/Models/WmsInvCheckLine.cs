using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsInvCheckLine
{
    public int CheckLineId { get; set; }

    public int? CheckId { get; set; }

    public int? VariantId { get; set; }

    public int? SystemQty { get; set; }

    public int? ActualQty { get; set; }

    public virtual WmsInvCheck? Check { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
