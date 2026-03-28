using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalReturnLine
{
    public int SreturnLineId { get; set; }

    public int? SreturnId { get; set; }

    public int? VariantId { get; set; }

    public int? ReturnQty { get; set; }

    public virtual SalReturn? Sreturn { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
