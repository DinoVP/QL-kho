using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalReturn
{
    public int SreturnId { get; set; }

    public string? ReturnCode { get; set; }

    public int? Doid { get; set; }

    public int? ReceiverId { get; set; }

    public virtual SalDelivery? Do { get; set; }

    public virtual SysUser? Receiver { get; set; }

    public virtual ICollection<SalReturnLine> SalReturnLines { get; set; } = new List<SalReturnLine>();
}
