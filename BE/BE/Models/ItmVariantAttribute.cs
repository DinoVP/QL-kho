using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmVariantAttribute
{
    public int VariantId { get; set; }

    public int AttrId { get; set; }

    public string? AttrValue { get; set; }

    public virtual ItmAttribute Attr { get; set; } = null!;

    public virtual ItmVariant Variant { get; set; } = null!;
}
