using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsWarehouse
{
    public int WarehouseId { get; set; }

    public int? BranchId { get; set; }

    public string? Whcode { get; set; }

    public string? Whname { get; set; }

    public virtual HrmBranch? Branch { get; set; }

    public virtual ICollection<PurReceipt> PurReceipts { get; set; } = new List<PurReceipt>();

    public virtual ICollection<SalDelivery> SalDeliveries { get; set; } = new List<SalDelivery>();

    public virtual ICollection<WmsDefect> WmsDefects { get; set; } = new List<WmsDefect>();

    public virtual ICollection<WmsInvCheck> WmsInvChecks { get; set; } = new List<WmsInvCheck>();

    public virtual ICollection<WmsStockBalance> WmsStockBalances { get; set; } = new List<WmsStockBalance>();

    public virtual ICollection<WmsStockLedger> WmsStockLedgers { get; set; } = new List<WmsStockLedger>();

    public virtual ICollection<WmsTransfer> WmsTransferFromWhNavigations { get; set; } = new List<WmsTransfer>();

    public virtual ICollection<WmsTransfer> WmsTransferToWhNavigations { get; set; } = new List<WmsTransfer>();

    public virtual ICollection<WmsZone> WmsZones { get; set; } = new List<WmsZone>();
}
