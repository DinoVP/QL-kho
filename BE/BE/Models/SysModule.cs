using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysModule
{
    public int ModuleId { get; set; }

    public string? ModuleCode { get; set; }

    public string? ModuleName { get; set; }

    public virtual ICollection<SysFeature> SysFeatures { get; set; } = new List<SysFeature>();
}
