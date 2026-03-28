using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysRole
{
    public int RoleId { get; set; }

    public string? RoleCode { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<SysFeature> Features { get; set; } = new List<SysFeature>();

    public virtual ICollection<SysUser> Users { get; set; } = new List<SysUser>();
}
