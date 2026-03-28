using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class HrmJobTitle
{
    public int TitleId { get; set; }

    public string? TitleCode { get; set; }

    public string? TitleName { get; set; }

    public virtual ICollection<HrmEmployee> HrmEmployees { get; set; } = new List<HrmEmployee>();
}
