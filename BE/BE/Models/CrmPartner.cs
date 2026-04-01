using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <-- QUAN TRỌNG

namespace BE.Models;

public partial class CrmPartner
{
    public int PartnerId { get; set; }
    public int? GroupId { get; set; }
    public string? PartnerCode { get; set; }
    public string? PartnerName { get; set; }
    public bool? IsSupplier { get; set; }
    public bool? IsCustomer { get; set; }

    // === CÁC CỘT MỚI THÊM ĐỂ KHỚP VỚI GIAO DIỆN VUE.JS ===
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }

    [NotMapped]
    public List<string>? AssignedSkus { get; set; } // Giữ tạm ở RAM để Frontend gán hàng
    // =======================================================

    [JsonIgnore]
    public virtual ICollection<CrmAddress> CrmAddresses { get; set; } = new List<CrmAddress>();
    [JsonIgnore]
    public virtual ICollection<CrmBankAccount> CrmBankAccounts { get; set; } = new List<CrmBankAccount>();
    [JsonIgnore]
    public virtual ICollection<CrmContact> CrmContacts { get; set; } = new List<CrmContact>();
    [JsonIgnore]
    public virtual ICollection<CrmContract> CrmContracts { get; set; } = new List<CrmContract>();
    [JsonIgnore]
    public virtual ICollection<CrmSupplierEval> CrmSupplierEvals { get; set; } = new List<CrmSupplierEval>();
    [JsonIgnore]
    public virtual ICollection<FinApInvoice> FinApInvoices { get; set; } = new List<FinApInvoice>();
    [JsonIgnore]
    public virtual ICollection<FinArInvoice> FinArInvoices { get; set; } = new List<FinArInvoice>();
    [JsonIgnore]
    public virtual ICollection<FinPayment> FinPayments { get; set; } = new List<FinPayment>();
    [JsonIgnore]
    public virtual ICollection<FinReceipt> FinReceipts { get; set; } = new List<FinReceipt>();

    [JsonIgnore]
    public virtual CrmPartnerGroup? Group { get; set; }

    [JsonIgnore]
    public virtual ICollection<ItmBatch> ItmBatches { get; set; } = new List<ItmBatch>();
    [JsonIgnore]
    public virtual ICollection<PurOrder> PurOrders { get; set; } = new List<PurOrder>();
    [JsonIgnore]
    public virtual ICollection<PurSupplierQuote> PurSupplierQuotes { get; set; } = new List<PurSupplierQuote>();
    [JsonIgnore]
    public virtual ICollection<SalOrder> SalOrders { get; set; } = new List<SalOrder>();
    [JsonIgnore]
    public virtual ICollection<SalQuotation> SalQuotations { get; set; } = new List<SalQuotation>();
}