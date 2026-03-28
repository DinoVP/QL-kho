using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmPayroll
{
    public int PayrollId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? SalaryMonth { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual HrmEmployee? Employee { get; set; }
}
