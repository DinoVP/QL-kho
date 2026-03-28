using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinCashAccount
{
    public int CashId { get; set; }

    public string? AccountCode { get; set; }

    public decimal? Balance { get; set; }

    public virtual ICollection<FinPayment> FinPayments { get; set; } = new List<FinPayment>();

    public virtual ICollection<FinReceipt> FinReceipts { get; set; } = new List<FinReceipt>();
}
