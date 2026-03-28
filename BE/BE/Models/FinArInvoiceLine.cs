using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinArInvoiceLine
{
    public int ArlineId { get; set; }

    public int? ArinvId { get; set; }

    public decimal? Amount { get; set; }

    public virtual FinArInvoice? Arinv { get; set; }
}
