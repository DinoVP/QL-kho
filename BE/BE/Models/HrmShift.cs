using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmShift
{
    public int ShiftId { get; set; }

    public string? ShiftName { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual ICollection<HrmTimesheet> HrmTimesheets { get; set; } = new List<HrmTimesheet>();
}
