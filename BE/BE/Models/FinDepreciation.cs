using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinDepreciation
{
    public long DepId { get; set; }

    public int? AssetId { get; set; }

    public decimal? DepAmount { get; set; }

    public virtual FinFixedAsset? Asset { get; set; }
}
