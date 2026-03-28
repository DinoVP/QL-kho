using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmAttribute
{
    public int AttrId { get; set; }

    public string? AttrName { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }

    public virtual ICollection<ItmVariantAttribute> ItmVariantAttributes { get; set; } = new List<ItmVariantAttribute>();
}
