using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsRack
{
    public int RackId { get; set; }

    public int? ZoneId { get; set; }

    public string? RackCode { get; set; }

    public virtual ICollection<WmsLocation> WmsLocations { get; set; } = new List<WmsLocation>();

    public virtual WmsZone? Zone { get; set; }
}
