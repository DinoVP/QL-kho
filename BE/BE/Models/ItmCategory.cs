using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmCategory
{
    public int CategoryId { get; set; }

    public string? CatCode { get; set; }

    public string? CatName { get; set; }

    public virtual ICollection<ItmSubCategory> ItmSubCategories { get; set; } = new List<ItmSubCategory>();
}
