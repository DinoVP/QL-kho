using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmUoMconversion
{
    public int ConvId { get; set; }

    public int? UoMgroupId { get; set; }

    public string? AltUoM { get; set; }

    public decimal? ConvRate { get; set; }

    public virtual ItmUoMgroup? UoMgroup { get; set; }
}
