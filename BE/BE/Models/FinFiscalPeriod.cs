using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinFiscalPeriod
{
    public int PeriodId { get; set; }

    public string? PeriodName { get; set; }

    public bool? IsClosed { get; set; }

    public int? ClosedBy { get; set; }

    public virtual SysUser? ClosedByNavigation { get; set; }
}
