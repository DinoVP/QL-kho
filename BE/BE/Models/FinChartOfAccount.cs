using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinChartOfAccount
{
    public int AccountId { get; set; }

    public string? AccountCode { get; set; }

    public string? AccountName { get; set; }

    public virtual ICollection<FinJournalLine> FinJournalLines { get; set; } = new List<FinJournalLine>();
}
