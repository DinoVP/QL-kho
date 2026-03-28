using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class LogManifestLine
{
    public int ManifestLineId { get; set; }

    public int? ManifestId { get; set; }

    public int? Doid { get; set; }

    public virtual SalDelivery? Do { get; set; }

    public virtual LogManifest? Manifest { get; set; }
}
