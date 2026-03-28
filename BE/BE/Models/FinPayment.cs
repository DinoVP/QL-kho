using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinPayment
{
    public int PaymentId { get; set; }

    public int? SupplierId { get; set; }

    public int? CashId { get; set; }

    public int? BankAccId { get; set; }

    public decimal? Amount { get; set; }

    public int? CreatorId { get; set; }

    public virtual FinBankAccount? BankAcc { get; set; }

    public virtual FinCashAccount? Cash { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual CrmPartner? Supplier { get; set; }
}
