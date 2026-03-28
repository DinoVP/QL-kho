using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsZone
{
    public int ZoneId { get; set; }

    public int? WarehouseId { get; set; }

    public string? ZoneCode { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }

    public virtual ICollection<WmsRack> WmsRacks { get; set; } = new List<WmsRack>();
}
