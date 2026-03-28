using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurSupplierQuoteLine
{
    public int QuoteLineId { get; set; }

    public int? QuoteId { get; set; }

    public int? VariantId { get; set; }

    public decimal? Price { get; set; }

    public virtual PurSupplierQuote? Quote { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
