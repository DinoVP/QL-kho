using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalDeliveryLine
{
    public int DolineId { get; set; }
    public int? Doid { get; set; }
    public int? SolineId { get; set; }
    public int? DeliveredQty { get; set; }

    // CÁC CỘT MỚI THÊM
    public int? VariantId { get; set; }
    public decimal? Price { get; set; }
    public int? LocationId { get; set; }

    public virtual SalDelivery? Do { get; set; }
    public virtual SalOrderLine? Soline { get; set; }
}