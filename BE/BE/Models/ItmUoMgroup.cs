using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmUoMgroup
{
    public int UoMgroupId { get; set; }

    public string? GroupName { get; set; }

    public string? BaseUoM { get; set; }

    public virtual ICollection<ItmProduct> ItmProducts { get; set; } = new List<ItmProduct>();

    public virtual ICollection<ItmUoMconversion> ItmUoMconversions { get; set; } = new List<ItmUoMconversion>();
}
