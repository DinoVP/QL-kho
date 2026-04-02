using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurReceiptLine
{
    public int GrnlineId { get; set; }
    public int? Grnid { get; set; }
    public int? PolineId { get; set; }
    public int? ReceivedQty { get; set; }

    // CÁC CỘT MỚI THÊM
    public int? VariantId { get; set; }
    public decimal? Price { get; set; }
    public int? LocationId { get; set; }

    // THÊM NSX VÀ HSD VÀO ĐÂY ĐỂ CONTROLLER KHÔNG BÁO LỖI
    public DateTime? Nsx { get; set; }
    public DateTime? Hsd { get; set; }

    public virtual PurReceipt? Grn { get; set; }
    public virtual PurOrderLine? Poline { get; set; }
}