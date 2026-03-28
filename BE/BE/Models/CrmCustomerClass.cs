using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmCustomerClass
{
    public int ClassId { get; set; }

    public string? ClassName { get; set; }

    public decimal? DiscountPercent { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }
}
