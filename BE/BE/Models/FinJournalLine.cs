using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinJournalLine
{
    public long LineId { get; set; }

    public long? JournalId { get; set; }

    public int? AccountId { get; set; }

    public decimal? Debit { get; set; }

    public decimal? Credit { get; set; }

    public virtual FinChartOfAccount? Account { get; set; }

    public virtual FinJournal? Journal { get; set; }
}
