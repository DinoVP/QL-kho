using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FinJournal
{
    public long JournalId { get; set; }

    public string? JournalNo { get; set; }

    public int? CreatorId { get; set; }

    public virtual SysUser? Creator { get; set; }

    public virtual ICollection<FinJournalLine> FinJournalLines { get; set; } = new List<FinJournalLine>();
}
