using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class LogDriver
{
    public int DriverId { get; set; }

    public int? EmployeeId { get; set; }

    public string? LicenseClass { get; set; }

    public virtual HrmEmployee? Employee { get; set; }

    public virtual ICollection<LogManifest> LogManifests { get; set; } = new List<LogManifest>();
}
