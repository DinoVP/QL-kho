using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsStockLedger
{
    public long LedgerId { get; set; }

    public int? WarehouseId { get; set; }

    public int? VariantId { get; set; }

    public string? RefCode { get; set; }

    public int? QtyChange { get; set; }

    public int? BalanceAfter { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ItmVariant? Variant { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }
}
