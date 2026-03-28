using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsLocationType
{
    public int LocTypeId { get; set; }

    public string? TypeName { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }
}
