using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmSubCategory
{
    public int SubCatId { get; set; }

    public int? CategoryId { get; set; }

    public string? SubCatName { get; set; }

    public virtual ItmCategory? Category { get; set; }

    public virtual ICollection<ItmProduct> ItmProducts { get; set; } = new List<ItmProduct>();
}
