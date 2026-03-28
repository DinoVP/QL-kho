using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinApInvoice
{
    public int ApinvId { get; set; }

    public int? SupplierId { get; set; }

    public int? Grnid { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<FinApInvoiceLine> FinApInvoiceLines { get; set; } = new List<FinApInvoiceLine>();

    public virtual PurReceipt? Grn { get; set; }

    public virtual CrmPartner? Supplier { get; set; }
}
