using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Đã thêm thư viện này để dùng Attribute [Column]

namespace BE.Models;

public partial class SysAuditLog
{
    public long LogId { get; set; }

    public int? UserId { get; set; }

    // Ép kiểu NVARCHAR dưới Database để lưu tiếng Việt có dấu
    [Column(TypeName = "nvarchar(255)")]
    public string? ActionType { get; set; }

    // Ép kiểu NVARCHAR dưới Database để lưu tiếng Việt có dấu
    [Column(TypeName = "nvarchar(500)")]
    public string? TableName { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual SysUser? User { get; set; }
}