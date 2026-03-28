using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmBankAccount
{
    public int BankId { get; set; }

    public int? PartnerId { get; set; }

    public string? BankName { get; set; }

    public string? AccountNo { get; set; }

    public virtual CrmPartner? Partner { get; set; }
}
