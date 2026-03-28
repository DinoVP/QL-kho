using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsTransfer
{
    public int TransferId { get; set; }

    public string? TransferCode { get; set; }

    public int? FromWh { get; set; }

    public int? ToWh { get; set; }

    public int? CreatorId { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual WmsWarehouse? FromWhNavigation { get; set; }

    public virtual WmsWarehouse? ToWhNavigation { get; set; }

    public virtual ICollection<WmsTransferLine> WmsTransferLines { get; set; } = new List<WmsTransferLine>();
}
