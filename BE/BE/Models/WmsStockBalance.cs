using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsStockBalance
{
    public long StockId { get; set; }

    public int? WarehouseId { get; set; }

    public int? LocationId { get; set; }

    public int? VariantId { get; set; }

    public int? Quantity { get; set; }

    // THÊM NSX VÀ HSD VÀO ĐÂY ĐỂ LƯU TỒN KHO THEO LÔ
    public DateTime? Nsx { get; set; }
    public DateTime? Hsd { get; set; }

    public virtual WmsLocation? Location { get; set; }

    public virtual ItmVariant? Variant { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }
}