using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsDefectLine
{
    public int DefectLineId { get; set; }
    public int? DefectId { get; set; }
    public int? VariantId { get; set; }
    public int? DefectQty { get; set; }

    // Các cột mới thêm
    public int? LocationId { get; set; }
    public string? Reason { get; set; }
    public DateTime? Nsx { get; set; }
    public DateTime? Hsd { get; set; }

    public virtual WmsDefect? Defect { get; set; }
    public virtual ItmVariant? Variant { get; set; }
}