using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurRequestLine
{
    public int PrlineId { get; set; }

    public int? Prid { get; set; }

    public int? VariantId { get; set; }

    public int? ReqQty { get; set; }

    public virtual PurRequest? Pr { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
