using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class LogRoute
{
    public int RouteId { get; set; }

    public string? RouteCode { get; set; }

    public string? RouteName { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<LogManifest> LogManifests { get; set; } = new List<LogManifest>();
}
