using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinCostCenter
{
    public int CenterId { get; set; }

    public string? CenterCode { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }
}
