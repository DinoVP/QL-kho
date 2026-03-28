using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurSupplierQuote
{
    public int QuoteId { get; set; }

    public int? SupplierId { get; set; }

    public DateOnly? QuoteDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<PurSupplierQuoteLine> PurSupplierQuoteLines { get; set; } = new List<PurSupplierQuoteLine>();

    public virtual CrmPartner? Supplier { get; set; }
}
