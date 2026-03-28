using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinTaxRate
{
    public int TaxId { get; set; }

    public string? TaxCode { get; set; }

    public decimal? TaxPercentage { get; set; }

    public virtual ICollection<ItmProduct> ItmProducts { get; set; } = new List<ItmProduct>();
}
