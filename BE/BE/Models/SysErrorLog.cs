using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysErrorLog
{
    public long ErrorId { get; set; }

    public int? UserId { get; set; }

    public string? ErrorMsg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SysUser? User { get; set; }
}
