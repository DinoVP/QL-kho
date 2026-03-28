using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmVariant
{
    public int VariantId { get; set; }

    public int? ProductId { get; set; }

    public string? VariantSku { get; set; }

    public string? Color { get; set; }

    public decimal? BasePrice { get; set; }

    public virtual ICollection<ItmBarcode> ItmBarcodes { get; set; } = new List<ItmBarcode>();

    public virtual ICollection<ItmBatch> ItmBatches { get; set; } = new List<ItmBatch>();

    public virtual ICollection<ItmPriceListDetail> ItmPriceListDetails { get; set; } = new List<ItmPriceListDetail>();

    public virtual ICollection<ItmSerial> ItmSerials { get; set; } = new List<ItmSerial>();

    public virtual ICollection<ItmVariantAttribute> ItmVariantAttributes { get; set; } = new List<ItmVariantAttribute>();

    public virtual ItmProduct? Product { get; set; }

    public virtual ICollection<PurOrderLine> PurOrderLines { get; set; } = new List<PurOrderLine>();

    public virtual ICollection<PurRequestLine> PurRequestLines { get; set; } = new List<PurRequestLine>();

    public virtual ICollection<PurReturnLine> PurReturnLines { get; set; } = new List<PurReturnLine>();

    public virtual ICollection<PurSupplierQuoteLine> PurSupplierQuoteLines { get; set; } = new List<PurSupplierQuoteLine>();

    public virtual ICollection<SalOrderLine> SalOrderLines { get; set; } = new List<SalOrderLine>();

    public virtual ICollection<SalQuotationLine> SalQuotationLines { get; set; } = new List<SalQuotationLine>();

    public virtual ICollection<SalReturnLine> SalReturnLines { get; set; } = new List<SalReturnLine>();

    public virtual ICollection<WmsAdjustmentLine> WmsAdjustmentLines { get; set; } = new List<WmsAdjustmentLine>();

    public virtual ICollection<WmsDefectLine> WmsDefectLines { get; set; } = new List<WmsDefectLine>();

    public virtual ICollection<WmsInvCheckLine> WmsInvCheckLines { get; set; } = new List<WmsInvCheckLine>();

    public virtual ICollection<WmsStockBalance> WmsStockBalances { get; set; } = new List<WmsStockBalance>();

    public virtual ICollection<WmsStockLedger> WmsStockLedgers { get; set; } = new List<WmsStockLedger>();

    public virtual ICollection<WmsTransferLine> WmsTransferLines { get; set; } = new List<WmsTransferLine>();
}
