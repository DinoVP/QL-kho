using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinApInvoiceLine
{
    public int AplineId { get; set; }

    public int? ApinvId { get; set; }

    public decimal? Amount { get; set; }

    public virtual FinApInvoice? Apinv { get; set; }
}
