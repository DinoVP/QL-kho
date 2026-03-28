using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmPartnerGroup
{
    public int GroupId { get; set; }

    public string? GroupCode { get; set; }

    public string? GroupName { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<CrmPartner> CrmPartners { get; set; } = new List<CrmPartner>();
}
