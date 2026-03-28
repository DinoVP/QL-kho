using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmTimesheet
{
    public long TimesheetId { get; set; }

    public int? EmployeeId { get; set; }

    public int? ShiftId { get; set; }

    public DateOnly? WorkDate { get; set; }

    public virtual HrmEmployee? Employee { get; set; }

    public virtual HrmShift? Shift { get; set; }
}
