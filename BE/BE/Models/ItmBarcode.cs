using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmBarcode
{
    public int BarcodeId { get; set; }

    public int? VariantId { get; set; }

    public string? Barcode { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
