using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class LogManifest
{
    public int ManifestId { get; set; }

    public int? VehicleId { get; set; }

    public int? DriverId { get; set; }

    public int? RouteId { get; set; }

    public int? DispatcherId { get; set; }

    public virtual SysUser? Dispatcher { get; set; }

    public virtual LogDriver? Driver { get; set; }

    public virtual ICollection<LogManifestLine> LogManifestLines { get; set; } = new List<LogManifestLine>();

    public virtual LogRoute? Route { get; set; }

    public virtual LogVehicle? Vehicle { get; set; }
}
