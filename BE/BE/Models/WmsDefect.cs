using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsDefect
{
    public int DefectId { get; set; }

    public string? DefectCode { get; set; }

    public int? WarehouseId { get; set; }

    public int? ReporterId { get; set; }

    public virtual SysUser? Reporter { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }

    public virtual ICollection<WmsDefectLine> WmsDefectLines { get; set; } = new List<WmsDefectLine>();
}
