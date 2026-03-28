using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinArInvoice
{
    public int ArinvId { get; set; }

    public int? CustomerId { get; set; }

    public int? Doid { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual CrmPartner? Customer { get; set; }

    public virtual SalDelivery? Do { get; set; }

    public virtual ICollection<FinArInvoiceLine> FinArInvoiceLines { get; set; } = new List<FinArInvoiceLine>();
}
