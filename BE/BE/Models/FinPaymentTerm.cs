using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinPaymentTerm
{
    public int TermId { get; set; }

    public string? TermCode { get; set; }

    public int? DaysDue { get; set; }

    public virtual ICollection<PurOrder> PurOrders { get; set; } = new List<PurOrder>();

    public virtual ICollection<SalOrder> SalOrders { get; set; } = new List<SalOrder>();
}
