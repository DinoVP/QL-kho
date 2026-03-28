using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmContract
{
    public int ContractId { get; set; }

    public int? PartnerId { get; set; }

    public string? ContractNo { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual CrmPartner? Partner { get; set; }
}
