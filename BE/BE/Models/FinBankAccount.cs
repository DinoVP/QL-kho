using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinBankAccount
{
    public int BankAccId { get; set; }

    public string? BankName { get; set; }

    public decimal? Balance { get; set; }

    public virtual ICollection<FinPayment> FinPayments { get; set; } = new List<FinPayment>();

    public virtual ICollection<FinReceipt> FinReceipts { get; set; } = new List<FinReceipt>();
}
