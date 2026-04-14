using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class PurOrder
{
    public int Poid { get; set; }
    public string? Pocode { get; set; }
    public int? SupplierId { get; set; }
    public int? TermId { get; set; }
    public string? Status { get; set; }

    // ==========================================
    // THÊM 3 CỘT NÀY ĐỂ ĐỒNG BỘ VỚI SQL TRÊN
    // ==========================================
    public string? Type { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Note { get; set; }
    // ==========================================

    public virtual ICollection<PurOrderLine> PurOrderLines { get; set; } = new List<PurOrderLine>();
    public virtual ICollection<PurReceipt> PurReceipts { get; set; } = new List<PurReceipt>();
    public virtual CrmPartner? Supplier { get; set; }
    public virtual FinPaymentTerm? Term { get; set; }
}