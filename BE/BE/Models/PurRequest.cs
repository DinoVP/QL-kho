using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurRequest
{
    public int Prid { get; set; }

    public string? Prcode { get; set; }

    public int? RequesterId { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PurRequestLine> PurRequestLines { get; set; } = new List<PurRequestLine>();

    public virtual HrmEmployee? Requester { get; set; }
}
