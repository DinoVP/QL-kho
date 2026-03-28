using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalQuotation
{
    public int Sqid { get; set; }

    public string? Sqcode { get; set; }

    public int? CustomerId { get; set; }

    public int? CreatorId { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual CrmPartner? Customer { get; set; }

    public virtual ICollection<SalQuotationLine> SalQuotationLines { get; set; } = new List<SalQuotationLine>();
}
