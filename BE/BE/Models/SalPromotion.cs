using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalPromotion
{
    public int PromoId { get; set; }

    public string? PromoCode { get; set; }

    public decimal? DiscountPercent { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<SalOrder> Sos { get; set; } = new List<SalOrder>();
}
