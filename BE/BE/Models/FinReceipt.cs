using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinReceipt
{
    public int ReceiptId { get; set; }

    public int? CustomerId { get; set; }

    public int? CashId { get; set; }

    public int? BankAccId { get; set; }

    public decimal? Amount { get; set; }

    public int? CreatorId { get; set; }

    public virtual FinBankAccount? BankAcc { get; set; }

    public virtual FinCashAccount? Cash { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual CrmPartner? Customer { get; set; }
}
