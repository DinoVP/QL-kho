using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurReceiptLine
{
    public int GrnlineId { get; set; }

    public int? Grnid { get; set; }

    public int? PolineId { get; set; }

    public int? ReceivedQty { get; set; }

    public virtual PurReceipt? Grn { get; set; }

    public virtual PurOrderLine? Poline { get; set; }
}
