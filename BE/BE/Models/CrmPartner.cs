using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmPartner
{
    public int PartnerId { get; set; }

    public int? GroupId { get; set; }

    public string? PartnerCode { get; set; }

    public string? PartnerName { get; set; }

    public bool? IsSupplier { get; set; }

    public bool? IsCustomer { get; set; }

    public virtual ICollection<CrmAddress> CrmAddresses { get; set; } = new List<CrmAddress>();

    public virtual ICollection<CrmBankAccount> CrmBankAccounts { get; set; } = new List<CrmBankAccount>();

    public virtual ICollection<CrmContact> CrmContacts { get; set; } = new List<CrmContact>();

    public virtual ICollection<CrmContract> CrmContracts { get; set; } = new List<CrmContract>();

    public virtual ICollection<CrmSupplierEval> CrmSupplierEvals { get; set; } = new List<CrmSupplierEval>();

    public virtual ICollection<FinApInvoice> FinApInvoices { get; set; } = new List<FinApInvoice>();

    public virtual ICollection<FinArInvoice> FinArInvoices { get; set; } = new List<FinArInvoice>();

    public virtual ICollection<FinPayment> FinPayments { get; set; } = new List<FinPayment>();

    public virtual ICollection<FinReceipt> FinReceipts { get; set; } = new List<FinReceipt>();

    public virtual CrmPartnerGroup? Group { get; set; }

    public virtual ICollection<ItmBatch> ItmBatches { get; set; } = new List<ItmBatch>();

    public virtual ICollection<PurOrder> PurOrders { get; set; } = new List<PurOrder>();

    public virtual ICollection<PurSupplierQuote> PurSupplierQuotes { get; set; } = new List<PurSupplierQuote>();

    public virtual ICollection<SalOrder> SalOrders { get; set; } = new List<SalOrder>();

    public virtual ICollection<SalQuotation> SalQuotations { get; set; } = new List<SalQuotation>();
}
