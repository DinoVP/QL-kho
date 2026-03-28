using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsAdjustment
{
    public int AdjId { get; set; }

    public string? AdjCode { get; set; }

    public int? CheckId { get; set; }

    public int? ApproverId { get; set; }

    public virtual SysUser? Approver { get; set; }

    public virtual WmsInvCheck? Check { get; set; }

    public virtual ICollection<WmsAdjustmentLine> WmsAdjustmentLines { get; set; } = new List<WmsAdjustmentLine>();
}
