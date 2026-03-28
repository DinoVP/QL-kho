using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ItmImage
{
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ItmProduct? Product { get; set; }
}
