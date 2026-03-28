using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalDelivery
{
    public int Doid { get; set; }

    public string? Docode { get; set; }

    public int? Soid { get; set; }

    public int? WarehouseId { get; set; }

    public int? DispatcherId { get; set; }

    public string? Status { get; set; }

    public virtual SysUser? Dispatcher { get; set; }

    public virtual ICollection<FinArInvoice> FinArInvoices { get; set; } = new List<FinArInvoice>();

    public virtual ICollection<LogManifestLine> LogManifestLines { get; set; } = new List<LogManifestLine>();

    public virtual ICollection<SalDeliveryLine> SalDeliveryLines { get; set; } = new List<SalDeliveryLine>();

    public virtual ICollection<SalReturn> SalReturns { get; set; } = new List<SalReturn>();

    public virtual SalOrder? So { get; set; }

    public virtual WmsWarehouse? Warehouse { get; set; }
}
