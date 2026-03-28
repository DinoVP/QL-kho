using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsLocation
{
    public int LocationId { get; set; }

    public int? RackId { get; set; }

    public string? LocationCode { get; set; }

    public int? MaxCapacity { get; set; }

    public virtual WmsRack? Rack { get; set; }

    public virtual ICollection<WmsStockBalance> WmsStockBalances { get; set; } = new List<WmsStockBalance>();
}
