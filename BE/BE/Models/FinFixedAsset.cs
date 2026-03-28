using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinFixedAsset
{
    public int AssetId { get; set; }

    public string? AssetCode { get; set; }

    public decimal? PurchasePrice { get; set; }

    public int? RecordedBy { get; set; }

    public virtual ICollection<FinDepreciation> FinDepreciations { get; set; } = new List<FinDepreciation>();

    public virtual SysUser? RecordedByNavigation { get; set; }
}
