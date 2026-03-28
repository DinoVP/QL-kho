using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmSerial
{
    public long SerialId { get; set; }

    public int? VariantId { get; set; }

    public string? SerialNo { get; set; }

    public string? Status { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
