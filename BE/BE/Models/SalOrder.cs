using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalOrder
{
    public int Soid { get; set; }

    public string? Socode { get; set; }

    public int? CustomerId { get; set; }

    public int? TermId { get; set; }

    public string? Status { get; set; }

    public virtual CrmPartner? Customer { get; set; }

    public virtual ICollection<SalDelivery> SalDeliveries { get; set; } = new List<SalDelivery>();

    public virtual ICollection<SalOrderLine> SalOrderLines { get; set; } = new List<SalOrderLine>();

    public virtual FinPaymentTerm? Term { get; set; }

    public virtual ICollection<SalPromotion> Promos { get; set; } = new List<SalPromotion>();
}
