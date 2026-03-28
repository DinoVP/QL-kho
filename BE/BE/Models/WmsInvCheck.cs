using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsInvCheck
{
    public int CheckId { get; set; }

    public string? CheckCode { get; set; }

    public int? WarehouseId { get; set; }

    public int? CheckerId { get; set; }

    public virtual HrmEmployee? Checker { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }

    public virtual ICollection<WmsAdjustment> WmsAdjustments { get; set; } = new List<WmsAdjustment>();

    public virtual ICollection<WmsInvCheckLine> WmsInvCheckLines { get; set; } = new List<WmsInvCheckLine>();
}
