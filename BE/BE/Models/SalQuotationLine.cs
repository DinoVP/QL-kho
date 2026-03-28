using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalQuotationLine
{
    public int SqlineId { get; set; }

    public int? Sqid { get; set; }

    public int? VariantId { get; set; }

    public int? Qty { get; set; }

    public decimal? Price { get; set; }

    public virtual SalQuotation? Sq { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
