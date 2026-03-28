using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class LogVehicle
{
    public int VehicleId { get; set; }

    public string? LicensePlate { get; set; }

    public decimal? Capacity { get; set; }

    public int? ManagedBy { get; set; }

    public virtual ICollection<LogManifest> LogManifests { get; set; } = new List<LogManifest>();

    public virtual SysUser? ManagedByNavigation { get; set; }
}
