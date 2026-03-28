using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysFeature
{
    public int FeatureId { get; set; }

    public int? ModuleId { get; set; }

    public string? FeatureCode { get; set; }

    public string? FeatureName { get; set; }

    public virtual SysModule? Module { get; set; }

    public virtual ICollection<SysRole> Roles { get; set; } = new List<SysRole>();
}
