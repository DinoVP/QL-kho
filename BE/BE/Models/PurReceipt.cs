using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurReceipt
{
    public int Grnid { get; set; }

    public string? Grncode { get; set; }

    public int? Poid { get; set; }

    public int? WarehouseId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<FinApInvoice> FinApInvoices { get; set; } = new List<FinApInvoice>();

    public virtual PurOrder? Po { get; set; }

    public virtual ICollection<PurReceiptLine> PurReceiptLines { get; set; } = new List<PurReceiptLine>();

    public virtual ICollection<PurReturn> PurReturns { get; set; } = new List<PurReturn>();

    public virtual SysUser? Receiver { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }
}
