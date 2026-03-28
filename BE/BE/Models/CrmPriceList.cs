using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmPriceList
{
    public int PriceListId { get; set; }

    public string? ListCode { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<ItmPriceListDetail> ItmPriceListDetails { get; set; } = new List<ItmPriceListDetail>();
}
