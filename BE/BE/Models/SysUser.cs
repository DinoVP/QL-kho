using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysUser
{
    public int UserId { get; set; }

    public int? EmployeeId { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CrmCustomerClass> CrmCustomerClasses { get; set; } = new List<CrmCustomerClass>();

    public virtual ICollection<CrmPartnerGroup> CrmPartnerGroups { get; set; } = new List<CrmPartnerGroup>();

    public virtual ICollection<CrmPriceList> CrmPriceLists { get; set; } = new List<CrmPriceList>();

    public virtual HrmEmployee? Employee { get; set; }

    public virtual ICollection<FinCostCenter> FinCostCenters { get; set; } = new List<FinCostCenter>();

    public virtual ICollection<FinFiscalPeriod> FinFiscalPeriods { get; set; } = new List<FinFiscalPeriod>();

    public virtual ICollection<FinFixedAsset> FinFixedAssets { get; set; } = new List<FinFixedAsset>();

    public virtual ICollection<FinJournal> FinJournals { get; set; } = new List<FinJournal>();

    public virtual ICollection<FinPayment> FinPayments { get; set; } = new List<FinPayment>();

    public virtual ICollection<FinReceipt> FinReceipts { get; set; } = new List<FinReceipt>();

    public virtual ICollection<ItmAttribute> ItmAttributes { get; set; } = new List<ItmAttribute>();

    public virtual ICollection<LogManifest> LogManifests { get; set; } = new List<LogManifest>();

    public virtual ICollection<LogRoute> LogRoutes { get; set; } = new List<LogRoute>();

    public virtual ICollection<LogVehicle> LogVehicles { get; set; } = new List<LogVehicle>();

    public virtual ICollection<PurReceipt> PurReceipts { get; set; } = new List<PurReceipt>();

    public virtual ICollection<PurReturn> PurReturns { get; set; } = new List<PurReturn>();

    public virtual ICollection<PurSupplierQuote> PurSupplierQuotes { get; set; } = new List<PurSupplierQuote>();

    public virtual ICollection<SalDelivery> SalDeliveries { get; set; } = new List<SalDelivery>();

    public virtual ICollection<SalPromotion> SalPromotions { get; set; } = new List<SalPromotion>();

    public virtual ICollection<SalQuotation> SalQuotations { get; set; } = new List<SalQuotation>();

    public virtual ICollection<SalReturn> SalReturns { get; set; } = new List<SalReturn>();

    public virtual ICollection<SysAuditLog> SysAuditLogs { get; set; } = new List<SysAuditLog>();

    public virtual ICollection<SysEmailTemplate> SysEmailTemplates { get; set; } = new List<SysEmailTemplate>();

    public virtual ICollection<SysErrorLog> SysErrorLogs { get; set; } = new List<SysErrorLog>();

    public virtual ICollection<SysSetting> SysSettings { get; set; } = new List<SysSetting>();

    public virtual ICollection<WmsAdjustment> WmsAdjustments { get; set; } = new List<WmsAdjustment>();

    public virtual ICollection<WmsDefect> WmsDefects { get; set; } = new List<WmsDefect>();

    public virtual ICollection<WmsLocationType> WmsLocationTypes { get; set; } = new List<WmsLocationType>();

    public virtual ICollection<WmsStockLedger> WmsStockLedgers { get; set; } = new List<WmsStockLedger>();

    public virtual ICollection<WmsTransfer> WmsTransfers { get; set; } = new List<WmsTransfer>();

    public virtual ICollection<SysRole> Roles { get; set; } = new List<SysRole>();
}
