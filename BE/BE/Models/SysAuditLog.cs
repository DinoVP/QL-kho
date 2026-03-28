using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysAuditLog
{
    public long LogId { get; set; }

    public int? UserId { get; set; }

    public string? ActionType { get; set; }

    public string? TableName { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual SysUser? User { get; set; }
}
