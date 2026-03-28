using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurReturnLine
{
    public int PreturnLineId { get; set; }

    public int? PreturnId { get; set; }

    public int? VariantId { get; set; }

    public int? ReturnQty { get; set; }

    public virtual PurReturn? Preturn { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
