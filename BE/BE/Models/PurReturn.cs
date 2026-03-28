using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurReturn
{
    public int PreturnId { get; set; }

    public string? ReturnCode { get; set; }

    public int? Grnid { get; set; }

    public int? CreatorId { get; set; }

    public string? Status { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual PurReceipt? Grn { get; set; }

    public virtual ICollection<PurReturnLine> PurReturnLines { get; set; } = new List<PurReturnLine>();
}
