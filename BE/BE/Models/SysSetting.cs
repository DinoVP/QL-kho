using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysSetting
{
    public int SettingId { get; set; }

    public string? SettingKey { get; set; }

    public string? SettingValue { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual SysUser? UpdatedByNavigation { get; set; }
}
