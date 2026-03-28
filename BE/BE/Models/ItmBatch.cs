using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmBatch
{
    public int BatchId { get; set; }

    public int? VariantId { get; set; }

    public int? SupplierId { get; set; }

    public string? BatchNo { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public virtual CrmPartner? Supplier { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
