using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Tên_Project_Của_Sếp.Models; // Sếp nhớ giữ nguyên namespace gốc của sếp chỗ này nhé nếu có khác biệt

namespace BE.Models;

public partial class QLKhoContext : DbContext
{
    public QLKhoContext()
    {
    }

    public QLKhoContext(DbContextOptions<QLKhoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CrmAddress> CrmAddresses { get; set; }
    public virtual DbSet<CrmBankAccount> CrmBankAccounts { get; set; }
    public virtual DbSet<CrmContact> CrmContacts { get; set; }
    public virtual DbSet<CrmContract> CrmContracts { get; set; }
    public virtual DbSet<CrmCustomerClass> CrmCustomerClasses { get; set; }
    public virtual DbSet<CrmPartner> CrmPartners { get; set; }
    public virtual DbSet<CrmPartnerGroup> CrmPartnerGroups { get; set; }
    public virtual DbSet<CrmPriceList> CrmPriceLists { get; set; }
    public virtual DbSet<CrmSupplierEval> CrmSupplierEvals { get; set; }
    public virtual DbSet<FinApInvoice> FinApInvoices { get; set; }
    public virtual DbSet<FinApInvoiceLine> FinApInvoiceLines { get; set; }
    public virtual DbSet<FinArInvoice> FinArInvoices { get; set; }
    public virtual DbSet<FinArInvoiceLine> FinArInvoiceLines { get; set; }
    public virtual DbSet<FinBankAccount> FinBankAccounts { get; set; }
    public virtual DbSet<FinCashAccount> FinCashAccounts { get; set; }
    public virtual DbSet<FinChartOfAccount> FinChartOfAccounts { get; set; }
    public virtual DbSet<FinCostCenter> FinCostCenters { get; set; }
    public virtual DbSet<FinDepreciation> FinDepreciations { get; set; }
    public virtual DbSet<FinFiscalPeriod> FinFiscalPeriods { get; set; }
    public virtual DbSet<FinFixedAsset> FinFixedAssets { get; set; }
    public virtual DbSet<FinJournal> FinJournals { get; set; }
    public virtual DbSet<FinJournalLine> FinJournalLines { get; set; }
    public virtual DbSet<FinPayment> FinPayments { get; set; }
    public virtual DbSet<FinPaymentTerm> FinPaymentTerms { get; set; }
    public virtual DbSet<FinReceipt> FinReceipts { get; set; }
    public virtual DbSet<FinTaxRate> FinTaxRates { get; set; }
    public virtual DbSet<GeoCountry> GeoCountries { get; set; }
    public virtual DbSet<GeoDistrict> GeoDistricts { get; set; }
    public virtual DbSet<GeoProvince> GeoProvinces { get; set; }
    public virtual DbSet<HrmBranch> HrmBranches { get; set; }
    public virtual DbSet<HrmDepartment> HrmDepartments { get; set; }
    public virtual DbSet<HrmEmployee> HrmEmployees { get; set; }
    public virtual DbSet<HrmJobTitle> HrmJobTitles { get; set; }
    public virtual DbSet<HrmPayroll> HrmPayrolls { get; set; }
    public virtual DbSet<HrmShift> HrmShifts { get; set; }
    public virtual DbSet<HrmTimesheet> HrmTimesheets { get; set; }
    public virtual DbSet<ItmAttribute> ItmAttributes { get; set; }
    public virtual DbSet<ItmBarcode> ItmBarcodes { get; set; }
    public virtual DbSet<ItmBatch> ItmBatches { get; set; }
    public virtual DbSet<ItmBrand> ItmBrands { get; set; }
    public virtual DbSet<ItmCategory> ItmCategories { get; set; }
    public virtual DbSet<ItmImage> ItmImages { get; set; }
    public virtual DbSet<ItmPriceListDetail> ItmPriceListDetails { get; set; }

    // ĐÃ BỔ SUNG BẢNG LỊCH SỬ GIÁ Ở ĐÂY
    public virtual DbSet<ItmPriceHistory> ItmPriceHistories { get; set; }

    public virtual DbSet<ItmProduct> ItmProducts { get; set; }
    public virtual DbSet<ItmSerial> ItmSerials { get; set; }
    public virtual DbSet<ItmSubCategory> ItmSubCategories { get; set; }

    // --- CÁC BẢNG QUY ĐỔI MỚI (Thay thế ItmUoMgroups/ItmUoMconversions) ---
    public virtual DbSet<ItmUnit> ItmUnits { get; set; }
    public virtual DbSet<ItmProductUnit> ItmProductUnits { get; set; }

    public virtual DbSet<ItmVariant> ItmVariants { get; set; }
    public virtual DbSet<ItmVariantAttribute> ItmVariantAttributes { get; set; }
    public virtual DbSet<LogDriver> LogDrivers { get; set; }
    public virtual DbSet<LogManifest> LogManifests { get; set; }
    public virtual DbSet<LogManifestLine> LogManifestLines { get; set; }
    public virtual DbSet<LogRoute> LogRoutes { get; set; }
    public virtual DbSet<LogVehicle> LogVehicles { get; set; }
    public virtual DbSet<PurOrder> PurOrders { get; set; }
    public virtual DbSet<PurOrderLine> PurOrderLines { get; set; }
    public virtual DbSet<PurReceipt> PurReceipts { get; set; }
    public virtual DbSet<PurReceiptLine> PurReceiptLines { get; set; }
    public virtual DbSet<PurRequest> PurRequests { get; set; }
    public virtual DbSet<PurRequestLine> PurRequestLines { get; set; }
    public virtual DbSet<PurReturn> PurReturns { get; set; }
    public virtual DbSet<PurReturnLine> PurReturnLines { get; set; }
    public virtual DbSet<PurSupplierQuote> PurSupplierQuotes { get; set; }
    public virtual DbSet<PurSupplierQuoteLine> PurSupplierQuoteLines { get; set; }
    public virtual DbSet<SalDelivery> SalDeliveries { get; set; }
    public virtual DbSet<SalDeliveryLine> SalDeliveryLines { get; set; }
    public virtual DbSet<SalOrder> SalOrders { get; set; }
    public virtual DbSet<SalOrderLine> SalOrderLines { get; set; }
    public virtual DbSet<SalPromotion> SalPromotions { get; set; }
    public virtual DbSet<SalQuotation> SalQuotations { get; set; }
    public virtual DbSet<SalQuotationLine> SalQuotationLines { get; set; }
    public virtual DbSet<SalReturn> SalReturns { get; set; }
    public virtual DbSet<SalReturnLine> SalReturnLines { get; set; }
    public virtual DbSet<SysAuditLog> SysAuditLogs { get; set; }
    public virtual DbSet<SysEmailTemplate> SysEmailTemplates { get; set; }
    public virtual DbSet<SysErrorLog> SysErrorLogs { get; set; }
    public virtual DbSet<SysFeature> SysFeatures { get; set; }
    public virtual DbSet<SysModule> SysModules { get; set; }
    public virtual DbSet<SysRole> SysRoles { get; set; }
    public virtual DbSet<SysSetting> SysSettings { get; set; }
    public virtual DbSet<SysUiLog> SysUiLogs { get; set; }
    public virtual DbSet<SysUser> SysUsers { get; set; }
    public virtual DbSet<WmsAdjustment> WmsAdjustments { get; set; }
    public virtual DbSet<WmsAdjustmentLine> WmsAdjustmentLines { get; set; }
    public virtual DbSet<WmsDefect> WmsDefects { get; set; }
    public virtual DbSet<WmsDefectLine> WmsDefectLines { get; set; }
    public virtual DbSet<WmsInvCheck> WmsInvChecks { get; set; }
    public virtual DbSet<WmsInvCheckLine> WmsInvCheckLines { get; set; }
    public virtual DbSet<WmsLocation> WmsLocations { get; set; }
    public virtual DbSet<WmsLocationType> WmsLocationTypes { get; set; }
    public virtual DbSet<WmsRack> WmsRacks { get; set; }
    public virtual DbSet<WmsStockBalance> WmsStockBalances { get; set; }
    public virtual DbSet<WmsStockLedger> WmsStockLedgers { get; set; }
    public virtual DbSet<WmsTransfer> WmsTransfers { get; set; }
    public virtual DbSet<WmsTransferLine> WmsTransferLines { get; set; }
    public virtual DbSet<WmsWarehouse> WmsWarehouses { get; set; }
    public virtual DbSet<WmsZone> WmsZones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrmAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__CRM_Addr__091C2A1BC2A12ECA");

            entity.ToTable("CRM_Addresses");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.AddressLine).HasMaxLength(500);
            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");

            entity.HasOne(d => d.District).WithMany(p => p.CrmAddresses)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK__CRM_Addre__Distr__08B54D69");

            entity.HasOne(d => d.Partner).WithMany(p => p.CrmAddresses)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__CRM_Addre__Partn__07C12930");
        });

        modelBuilder.Entity<CrmBankAccount>(entity =>
        {
            entity.HasKey(e => e.BankId).HasName("PK__CRM_Bank__AA08CB33FB5ED6D6");

            entity.ToTable("CRM_BankAccounts");

            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BankName).HasMaxLength(150);
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");

            entity.HasOne(d => d.Partner).WithMany(p => p.CrmBankAccounts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__CRM_BankA__Partn__0E6E26BF");
        });

        modelBuilder.Entity<CrmContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__CRM_Cont__5C6625BB3D1091CC");

            entity.ToTable("CRM_Contacts");

            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContactName).HasMaxLength(150);
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Partner).WithMany(p => p.CrmContacts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__CRM_Conta__Partn__0B91BA14");
        });

        modelBuilder.Entity<CrmContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__CRM_Cont__C90D3409730F598C");

            entity.ToTable("CRM_Contracts");

            entity.HasIndex(e => e.ContractNo, "UQ__CRM_Cont__C908F4B8EEAEE777").IsUnique();

            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.ContractNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");

            entity.HasOne(d => d.Partner).WithMany(p => p.CrmContracts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__CRM_Contr__Partn__18EBB532");
        });

        modelBuilder.Entity<CrmCustomerClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__CRM_Cust__CB1927A09B86E037");

            entity.ToTable("CRM_CustomerClasses");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CrmCustomerClasses)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__CRM_Custo__Creat__151B244E");
        });

        modelBuilder.Entity<CrmPartner>(entity =>
        {
            entity.HasKey(e => e.PartnerId).HasName("PK__CRM_Part__39FD6332AEDA337F");

            entity.ToTable("CRM_Partners");

            entity.HasIndex(e => e.PartnerCode, "UQ__CRM_Part__E6792F57A24E7A17").IsUnique();

            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.PartnerCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartnerName).HasMaxLength(255);

            entity.HasOne(d => d.Group).WithMany(p => p.CrmPartners)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__CRM_Partn__Group__04E4BC85");
        });

        modelBuilder.Entity<CrmPartnerGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__CRM_Part__149AF30A0EC8D356");

            entity.ToTable("CRM_PartnerGroups");

            entity.HasIndex(e => e.GroupCode, "UQ__CRM_Part__3B9743800E94A435").IsUnique();

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.GroupCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GroupName).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CrmPartnerGroups)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__CRM_Partn__Creat__01142BA1");
        });

        modelBuilder.Entity<CrmPriceList>(entity =>
        {
            entity.HasKey(e => e.PriceListId).HasName("PK__CRM_Pric__1E30F34C48BB0AAE");

            entity.ToTable("CRM_PriceLists");

            entity.HasIndex(e => e.ListCode, "UQ__CRM_Pric__287292B8AA303233").IsUnique();

            entity.Property(e => e.PriceListId).HasColumnName("PriceListID");
            entity.Property(e => e.ListCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CrmPriceLists)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__CRM_Price__Creat__1CBC4616");
        });

        modelBuilder.Entity<CrmSupplierEval>(entity =>
        {
            entity.HasKey(e => e.EvalId).HasName("PK__CRM_Supp__C1A298ADDFD6C296");

            entity.ToTable("CRM_SupplierEvals");

            entity.Property(e => e.EvalId).HasColumnName("EvalID");
            entity.Property(e => e.EvaluatorId).HasColumnName("EvaluatorID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

            entity.HasOne(d => d.Evaluator).WithMany(p => p.CrmSupplierEvals)
                .HasForeignKey(d => d.EvaluatorId)
                .HasConstraintName("FK__CRM_Suppl__Evalu__123EB7A3");

            entity.HasOne(d => d.Supplier).WithMany(p => p.CrmSupplierEvals)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__CRM_Suppl__Suppl__114A936A");
        });

        modelBuilder.Entity<FinApInvoice>(entity =>
        {
            entity.HasKey(e => e.ApinvId).HasName("PK__FIN_AP_I__A3A91459494E1DD7");

            entity.ToTable("FIN_AP_Invoices");

            entity.Property(e => e.ApinvId).HasColumnName("APInvID");
            entity.Property(e => e.Grnid).HasColumnName("GRNID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Grn).WithMany(p => p.FinApInvoices)
                .HasForeignKey(d => d.Grnid)
                .HasConstraintName("FK__FIN_AP_In__GRNID__7CD98669");

            entity.HasOne(d => d.Supplier).WithMany(p => p.FinApInvoices)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__FIN_AP_In__Suppl__7BE56230");
        });

        modelBuilder.Entity<FinApInvoiceLine>(entity =>
        {
            entity.HasKey(e => e.AplineId).HasName("PK__FIN_AP_I__3AFB7CDB9B17A966");

            entity.ToTable("FIN_AP_InvoiceLines");

            entity.Property(e => e.AplineId).HasColumnName("APLineID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ApinvId).HasColumnName("APInvID");

            entity.HasOne(d => d.Apinv).WithMany(p => p.FinApInvoiceLines)
                .HasForeignKey(d => d.ApinvId)
                .HasConstraintName("FK__FIN_AP_In__APInv__7FB5F314");
        });

        modelBuilder.Entity<FinArInvoice>(entity =>
        {
            entity.HasKey(e => e.ArinvId).HasName("PK__FIN_AR_I__BFD2EBB5AA37E360");

            entity.ToTable("FIN_AR_Invoices");

            entity.Property(e => e.ArinvId).HasColumnName("ARInvID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Doid).HasColumnName("DOID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.FinArInvoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__FIN_AR_In__Custo__02925FBF");

            entity.HasOne(d => d.Do).WithMany(p => p.FinArInvoices)
                .HasForeignKey(d => d.Doid)
                .HasConstraintName("FK__FIN_AR_Inv__DOID__038683F8");
        });

        modelBuilder.Entity<FinArInvoiceLine>(entity =>
        {
            entity.HasKey(e => e.ArlineId).HasName("PK__FIN_AR_I__A7CE4CBB370518DB");

            entity.ToTable("FIN_AR_InvoiceLines");

            entity.Property(e => e.ArlineId).HasColumnName("ARLineID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ArinvId).HasColumnName("ARInvID");

            entity.HasOne(d => d.Arinv).WithMany(p => p.FinArInvoiceLines)
                .HasForeignKey(d => d.ArinvId)
                .HasConstraintName("FK__FIN_AR_In__ARInv__0662F0A3");
        });

        modelBuilder.Entity<FinBankAccount>(entity =>
        {
            entity.HasKey(e => e.BankAccId).HasName("PK__FIN_Bank__BAB4EDD4C16D448C");

            entity.ToTable("FIN_BankAccounts");

            entity.Property(e => e.BankAccId).HasColumnName("BankAccID");
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankName).HasMaxLength(150);
        });

        modelBuilder.Entity<FinCashAccount>(entity =>
        {
            entity.HasKey(e => e.CashId).HasName("PK__FIN_Cash__6B801A4B8487B873");

            entity.ToTable("FIN_CashAccounts");

            entity.HasIndex(e => e.AccountCode, "UQ__FIN_Cash__38D0C56A6405293D").IsUnique();

            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.AccountCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<FinChartOfAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__FIN_Char__349DA5860168F545");

            entity.ToTable("FIN_ChartOfAccounts");

            entity.HasIndex(e => e.AccountCode, "UQ__FIN_Char__38D0C56A577D30C8").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.AccountCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AccountName).HasMaxLength(200);
        });

        modelBuilder.Entity<FinCostCenter>(entity =>
        {
            entity.HasKey(e => e.CenterId).HasName("PK__FIN_Cost__398FC7D7E47FC390");

            entity.ToTable("FIN_CostCenters");

            entity.HasIndex(e => e.CenterCode, "UQ__FIN_Cost__55D5E3C6237E870D").IsUnique();

            entity.Property(e => e.CenterId).HasColumnName("CenterID");
            entity.Property(e => e.CenterCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FinCostCenters)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__FIN_CostC__Creat__69C6B1F5");
        });

        modelBuilder.Entity<FinDepreciation>(entity =>
        {
            entity.HasKey(e => e.DepId).HasName("PK__FIN_Depr__DB9CAA7F2E3FF781");

            entity.ToTable("FIN_Depreciations");

            entity.Property(e => e.DepId).HasColumnName("DepID");
            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.DepAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Asset).WithMany(p => p.FinDepreciations)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK__FIN_Depre__Asset__1881A0DE");
        });

        modelBuilder.Entity<FinFiscalPeriod>(entity =>
        {
            entity.HasKey(e => e.PeriodId).HasName("PK__FIN_Fisc__E521BB3627EFCA1C");

            entity.ToTable("FIN_FiscalPeriods");

            entity.Property(e => e.PeriodId).HasColumnName("PeriodID");
            entity.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ClosedByNavigation).WithMany(p => p.FinFiscalPeriods)
                .HasForeignKey(d => d.ClosedBy)
                .HasConstraintName("FK__FIN_Fisca__Close__6CA31EA0");
        });

        modelBuilder.Entity<FinFixedAsset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__FIN_Fixe__43492372B61FBC6E");

            entity.ToTable("FIN_FixedAssets");

            entity.HasIndex(e => e.AssetCode, "UQ__FIN_Fixe__2DDE5240F6FF20CA").IsUnique();

            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.AssetCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.RecordedByNavigation).WithMany(p => p.FinFixedAssets)
                .HasForeignKey(d => d.RecordedBy)
                .HasConstraintName("FK__FIN_Fixed__Recor__15A53433");
        });

        modelBuilder.Entity<FinJournal>(entity =>
        {
            entity.HasKey(e => e.JournalId).HasName("PK__FIN_Jour__250103868F212644");

            entity.ToTable("FIN_Journals");

            entity.HasIndex(e => e.JournalNo, "UQ__FIN_Jour__250148D5BF949E58").IsUnique();

            entity.Property(e => e.JournalId).HasColumnName("JournalID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.JournalNo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Creator).WithMany(p => p.FinJournals)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__FIN_Journ__Creat__7073AF84");
        });

        modelBuilder.Entity<FinJournalLine>(entity =>
        {
            entity.HasKey(e => e.LineId).HasName("PK__FIN_Jour__2EAE64C9696C8EF6");

            entity.ToTable("FIN_JournalLines");

            entity.Property(e => e.LineId).HasColumnName("LineID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Credit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Debit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.JournalId).HasColumnName("JournalID");

            entity.HasOne(d => d.Account).WithMany(p => p.FinJournalLines)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__FIN_Journ__Accou__74444068");

            entity.HasOne(d => d.Journal).WithMany(p => p.FinJournalLines)
                .HasForeignKey(d => d.JournalId)
                .HasConstraintName("FK__FIN_Journ__Journ__73501C2F");
        });

        modelBuilder.Entity<FinPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__FIN_Paym__9B556A5861479487");

            entity.ToTable("FIN_Payments");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankAccId).HasColumnName("BankAccID");
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

            entity.HasOne(d => d.BankAcc).WithMany(p => p.FinPayments)
                .HasForeignKey(d => d.BankAccId)
                .HasConstraintName("FK__FIN_Payme__BankA__0B27A5C0");

            entity.HasOne(d => d.Cash).WithMany(p => p.FinPayments)
                .HasForeignKey(d => d.CashId)
                .HasConstraintName("FK__FIN_Payme__CashI__0A338187");

            entity.HasOne(d => d.Creator).WithMany(p => p.FinPayments)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__FIN_Payme__Creat__0C1BC9F9");

            entity.HasOne(d => d.Supplier).WithMany(p => p.FinPayments)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__FIN_Payme__Suppl__093F5D4E");
        });

        modelBuilder.Entity<FinPaymentTerm>(entity =>
        {
            entity.HasKey(e => e.TermId).HasName("PK__FIN_Paym__410A2E456A6B7436");

            entity.ToTable("FIN_PaymentTerms");

            entity.HasIndex(e => e.TermCode, "UQ__FIN_Paym__675CC10D88F7B665").IsUnique();

            entity.Property(e => e.TermId).HasColumnName("TermID");
            entity.Property(e => e.TermCode)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FinReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__FIN_Rece__CC08C400C99D6CAA");

            entity.ToTable("FIN_Receipts");

            entity.Property(e => e.ReceiptId).HasColumnName("ReceiptID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankAccId).HasColumnName("BankAccID");
            entity.Property(e => e.CashId).HasColumnName("CashID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.HasOne(d => d.BankAcc).WithMany(p => p.FinReceipts)
                .HasForeignKey(d => d.BankAccId)
                .HasConstraintName("FK__FIN_Recei__BankA__10E07F16");

            entity.HasOne(d => d.Cash).WithMany(p => p.FinReceipts)
                .HasForeignKey(d => d.CashId)
                .HasConstraintName("FK__FIN_Recei__CashI__0FEC5ADD");

            entity.HasOne(d => d.Creator).WithMany(p => p.FinReceipts)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__FIN_Recei__Creat__11D4A34F");

            entity.HasOne(d => d.Customer).WithMany(p => p.FinReceipts)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__FIN_Recei__Custo__0EF836A4");
        });

        modelBuilder.Entity<FinTaxRate>(entity =>
        {
            entity.HasKey(e => e.TaxId).HasName("PK__FIN_TaxR__711BE08CE2093224");

            entity.ToTable("FIN_TaxRates");

            entity.HasIndex(e => e.TaxCode, "UQ__FIN_TaxR__12945A28CDE4F663").IsUnique();

            entity.Property(e => e.TaxId).HasColumnName("TaxID");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TaxPercentage).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<GeoCountry>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK__GEO_Coun__10D160BF10F4479C");

            entity.ToTable("GEO_Countries");

            entity.HasIndex(e => e.CountryCode, "UQ__GEO_Coun__5D9B0D2C57D8C954").IsUnique();

            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CountryName).HasMaxLength(100);
        });

        modelBuilder.Entity<GeoDistrict>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__GEO_Dist__85FDA4A687B059BA");

            entity.ToTable("GEO_Districts");

            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");
            entity.Property(e => e.DistrictName).HasMaxLength(100);
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

            entity.HasOne(d => d.Province).WithMany(p => p.GeoDistricts)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK__GEO_Distr__Provi__3D5E1FD2");
        });

        modelBuilder.Entity<GeoProvince>(entity =>
        {
            entity.HasKey(e => e.ProvinceId).HasName("PK__GEO_Prov__FD0A6FA3A993363C");

            entity.ToTable("GEO_Provinces");

            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.ProvinceName).HasMaxLength(100);

            entity.HasOne(d => d.Country).WithMany(p => p.GeoProvinces)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK__GEO_Provi__Count__3A81B327");
        });

        modelBuilder.Entity<HrmBranch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__HRM_Bran__A1682FA589BF5E3E");

            entity.ToTable("HRM_Branches");

            entity.HasIndex(e => e.BranchCode, "UQ__HRM_Bran__1C61B8880DCD4BFC").IsUnique();

            entity.Property(e => e.BranchId).HasColumnName("BranchID");
            entity.Property(e => e.BranchCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BranchName).HasMaxLength(200);
            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

            entity.HasOne(d => d.District).WithMany(p => p.HrmBranches)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK__HRM_Branc__Distr__412EB0B6");
        });

        modelBuilder.Entity<HrmDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__HRM_Depa__B2079BCD73AFAD23");

            entity.ToTable("HRM_Departments");

            entity.HasIndex(e => e.DeptCode, "UQ__HRM_Depa__BB9B9550C5C793F9").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.BranchId).HasColumnName("BranchID");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DeptName).HasMaxLength(200);

            entity.HasOne(d => d.Branch).WithMany(p => p.HrmDepartments)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__HRM_Depar__Branc__44FF419A");
        });

        modelBuilder.Entity<HrmEmployee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__HRM_Empl__7AD04FF137B10A91");

            entity.ToTable("HRM_Employees");

            entity.HasIndex(e => e.EmpCode, "UQ__HRM_Empl__7DA847CA6FA867AC").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.TitleId).HasColumnName("TitleID");

            entity.HasOne(d => d.Department).WithMany(p => p.HrmEmployees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__HRM_Emplo__Depar__4BAC3F29");

            entity.HasOne(d => d.Title).WithMany(p => p.HrmEmployees)
                .HasForeignKey(d => d.TitleId)
                .HasConstraintName("FK__HRM_Emplo__Title__4CA06362");
        });

        modelBuilder.Entity<HrmJobTitle>(entity =>
        {
            entity.HasKey(e => e.TitleId).HasName("PK__HRM_JobT__757589E645F91538");

            entity.ToTable("HRM_JobTitles");

            entity.HasIndex(e => e.TitleCode, "UQ__HRM_JobT__8B388717B742A043").IsUnique();

            entity.Property(e => e.TitleId).HasColumnName("TitleID");
            entity.Property(e => e.TitleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TitleName).HasMaxLength(200);
        });

        modelBuilder.Entity<HrmPayroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__HRM_Payr__99DFC6924102B046");

            entity.ToTable("HRM_Payroll");

            entity.Property(e => e.PayrollId).HasColumnName("PayrollID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.HrmPayrolls)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__HRM_Payro__Emplo__5535A963");
        });

        modelBuilder.Entity<HrmShift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__HRM_Shif__C0A838E1A6E7FAA4");

            entity.ToTable("HRM_Shifts");

            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.ShiftName).HasMaxLength(100);
        });

        modelBuilder.Entity<HrmTimesheet>(entity =>
        {
            entity.HasKey(e => e.TimesheetId).HasName("PK__HRM_Time__848CBECD25C3AE1A");

            entity.ToTable("HRM_Timesheets");

            entity.Property(e => e.TimesheetId).HasColumnName("TimesheetID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");

            entity.HasOne(d => d.Employee).WithMany(p => p.HrmTimesheets)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__HRM_Times__Emplo__5165187F");

            entity.HasOne(d => d.Shift).WithMany(p => p.HrmTimesheets)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__HRM_Times__Shift__52593CB8");
        });

        modelBuilder.Entity<ItmAttribute>(entity =>
        {
            entity.HasKey(e => e.AttrId).HasName("PK__ITM_Attr__0108336FEA12A4B9");

            entity.ToTable("ITM_Attributes");

            entity.Property(e => e.AttrId).HasColumnName("AttrID");
            entity.Property(e => e.AttrName).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ItmAttributes)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__ITM_Attri__Creat__4C6B5938");
        });

        modelBuilder.Entity<ItmBarcode>(entity =>
        {
            entity.HasKey(e => e.BarcodeId).HasName("PK__ITM_Barc__21916C88414D95B5");

            entity.ToTable("ITM_Barcodes");

            entity.HasIndex(e => e.Barcode, "UQ__ITM_Barc__177800D347B8B07A").IsUnique();

            entity.Property(e => e.BarcodeId).HasColumnName("BarcodeID");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Variant).WithMany(p => p.ItmBarcodes)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__ITM_Barco__Varia__3B40CD36");
        });

        modelBuilder.Entity<ItmBatch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__ITM_Batc__5D55CE3894C6E6DD");

            entity.ToTable("ITM_Batches");

            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.BatchNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ItmBatches)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__ITM_Batch__Suppl__45BE5BA9");

            entity.HasOne(d => d.Variant).WithMany(p => p.ItmBatches)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__ITM_Batch__Varia__44CA3770");
        });

        modelBuilder.Entity<ItmBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__ITM_Bran__DAD4F3BE4D6EE7C0");

            entity.ToTable("ITM_Brands");

            entity.HasIndex(e => e.BrandCode, "UQ__ITM_Bran__44292CC70A7E54BB").IsUnique();

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.BrandCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BrandName).HasMaxLength(100);
        });

        modelBuilder.Entity<ItmCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ITM_Cate__19093A2B3F732579");

            entity.ToTable("ITM_Categories");

            entity.HasIndex(e => e.CatCode, "UQ__ITM_Cate__5E593E4EE9ADD03C").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CatCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CatName).HasMaxLength(200);
        });

        modelBuilder.Entity<ItmImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ITM_Imag__7516F4ECDCC9837E");

            entity.ToTable("ITM_Images");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.ItmImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ITM_Image__Produ__3E1D39E1");
        });

        modelBuilder.Entity<ItmPriceListDetail>(entity =>
        {
            entity.HasKey(e => new { e.PriceListId, e.VariantId }).HasName("PK__ITM_Pric__4EDAD072E33280D3");

            entity.ToTable("ITM_PriceListDetails");

            entity.Property(e => e.PriceListId).HasColumnName("PriceListID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.PriceList).WithMany(p => p.ItmPriceListDetails)
                .HasForeignKey(d => d.PriceListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ITM_Price__Price__40F9A68C");

            entity.HasOne(d => d.Variant).WithMany(p => p.ItmPriceListDetails)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ITM_Price__Varia__41EDCAC5");
        });

        // ĐÃ CẤU HÌNH FLUENT API CHO BẢNG LỊCH SỬ GIÁ
        modelBuilder.Entity<ItmPriceHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK_ITM_PriceHistories");
            entity.ToTable("ITM_PriceHistories");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NewPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.Source).HasMaxLength(255);

            entity.HasOne(d => d.Variant)
                .WithMany() // Không cần tạo ICollection trong Variant cho đỡ rối
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ITM_PriceHistories_Variants");
        });

        modelBuilder.Entity<ItmProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__ITM_Prod__B40CC6ED666D63E6");

            entity.ToTable("ITM_Products");

            entity.HasIndex(e => e.Sku, "UQ__ITM_Prod__CA1ECF0DD1C8A883").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.SubCatId).HasColumnName("SubCatID");
            entity.Property(e => e.TaxId).HasColumnName("TaxID");

            entity.HasOne(d => d.Brand).WithMany(p => p.ItmProducts)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__ITM_Produ__Brand__31B762FC");

            entity.HasOne(d => d.SubCat).WithMany(p => p.ItmProducts)
                .HasForeignKey(d => d.SubCatId)
                .HasConstraintName("FK__ITM_Produ__SubCa__30C33EC3");

            entity.HasOne(d => d.Tax).WithMany(p => p.ItmProducts)
                .HasForeignKey(d => d.TaxId)
                .HasConstraintName("FK__ITM_Produ__TaxID__339FAB6E");
        });

        // --- ĐÃ THÊM LẠI CẤU HÌNH CHO 2 BẢNG QUY ĐỔI MỚI (Units & ProductUnits) ---
        modelBuilder.Entity<ItmUnit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PK_ITM_Units");
            entity.ToTable("ITM_Units");

            entity.Property(e => e.UnitId).HasColumnName("UnitID");
            entity.HasIndex(e => e.UnitCode, "UQ_ITM_Units_UnitCode").IsUnique();
            entity.Property(e => e.UnitCode).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.UnitName).HasMaxLength(100);
        });

        modelBuilder.Entity<ItmProductUnit>(entity =>
        {
            entity.HasKey(e => e.ConversionId).HasName("PK_ITM_ProductUnits");
            entity.ToTable("ITM_ProductUnits");

            entity.Property(e => e.ConversionId).HasColumnName("ConversionID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.FromUnitId).HasColumnName("FromUnitID");
            entity.Property(e => e.ToUnitId).HasColumnName("ToUnitID");
            entity.Property(e => e.FromQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ToQty).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.ItmProductUnits)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ITM_ProductUnits_Product");

            entity.HasOne(d => d.FromUnit)
                .WithMany(p => p.FromProductUnits)
                .HasForeignKey(d => d.FromUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ITM_ProductUnits_FromUnit");

            entity.HasOne(d => d.ToUnit)
                .WithMany(p => p.ToProductUnits)
                .HasForeignKey(d => d.ToUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ITM_ProductUnits_ToUnit");
        });

        // --- ĐÃ KHÔI PHỤC LẠI CHO ITMSERIAL VÀ ITMSUBCATEGORY MÀ LÚC NÃY LỠ XÓA ---
        modelBuilder.Entity<ItmSerial>(entity =>
        {
            entity.HasKey(e => e.SerialId).HasName("PK__ITM_Seri__5E5B3EC4B26D29D0");
            entity.ToTable("ITM_Serials");
            entity.HasIndex(e => e.SerialNo, "UQ__ITM_Seri__5E5A535E27403A6B").IsUnique();
            entity.Property(e => e.SerialId).HasColumnName("SerialID");
            entity.Property(e => e.SerialNo).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.HasOne(d => d.Variant).WithMany(p => p.ItmSerials)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__ITM_Seria__Varia__498EEC8D");
        });

        modelBuilder.Entity<ItmSubCategory>(entity =>
        {
            entity.HasKey(e => e.SubCatId).HasName("PK__ITM_SubC__3963797575A02B0A");
            entity.ToTable("ITM_SubCategories");
            entity.Property(e => e.SubCatId).HasColumnName("SubCatID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.SubCatName).HasMaxLength(200);
            entity.HasOne(d => d.Category).WithMany(p => p.ItmSubCategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__ITM_SubCa__Categ__25518C17");
        });
        // -------------------------------------------------------------------------

        modelBuilder.Entity<ItmVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ITM_Vari__0EA233E43D9184DE");

            entity.ToTable("ITM_Variants");

            entity.HasIndex(e => e.VariantSku, "UQ__ITM_Vari__E55CDF9721810930").IsUnique();

            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.VariantSku)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("VariantSKU");

            entity.HasOne(d => d.Product).WithMany(p => p.ItmVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ITM_Varia__Produ__37703C52");
        });

        modelBuilder.Entity<ItmVariantAttribute>(entity =>
        {
            entity.HasKey(e => new { e.VariantId, e.AttrId }).HasName("PK__ITM_Vari__EEB2B0D23A6D993F");

            entity.ToTable("ITM_VariantAttributes");

            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.AttrId).HasColumnName("AttrID");
            entity.Property(e => e.AttrValue).HasMaxLength(100);

            entity.HasOne(d => d.Attr).WithMany(p => p.ItmVariantAttributes)
                .HasForeignKey(d => d.AttrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ITM_Varia__AttrI__503BEA1C");

            entity.HasOne(d => d.Variant).WithMany(p => p.ItmVariantAttributes)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ITM_Varia__Varia__4F47C5E3");
        });

        modelBuilder.Entity<LogDriver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK__LOG_Driv__F1B1CD247D58B636");

            entity.ToTable("LOG_Drivers");

            entity.Property(e => e.DriverId).HasColumnName("DriverID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LicenseClass)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithMany(p => p.LogDrivers)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__LOG_Drive__Emplo__1F2E9E6D");
        });

        modelBuilder.Entity<LogManifest>(entity =>
        {
            entity.HasKey(e => e.ManifestId).HasName("PK__LOG_Mani__6064F346D109ACEC");

            entity.ToTable("LOG_Manifests");

            entity.Property(e => e.ManifestId).HasColumnName("ManifestID");
            entity.Property(e => e.DispatcherId).HasColumnName("DispatcherID");
            entity.Property(e => e.DriverId).HasColumnName("DriverID");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

            entity.HasOne(d => d.Dispatcher).WithMany(p => p.LogManifests)
                .HasForeignKey(d => d.DispatcherId)
                .HasConstraintName("FK__LOG_Manif__Dispa__28B808A7");

            entity.HasOne(d => d.Driver).WithMany(p => p.LogManifests)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK__LOG_Manif__Drive__26CFC035");

            entity.HasOne(d => d.Route).WithMany(p => p.LogManifests)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK__LOG_Manif__Route__27C3E46E");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.LogManifests)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__LOG_Manif__Vehic__25DB9BFC");
        });

        modelBuilder.Entity<LogManifestLine>(entity =>
        {
            entity.HasKey(e => e.ManifestLineId).HasName("PK__LOG_Mani__19271EBEE1D003B2");

            entity.ToTable("LOG_ManifestLines");

            entity.Property(e => e.ManifestLineId).HasColumnName("ManifestLineID");
            entity.Property(e => e.Doid).HasColumnName("DOID");
            entity.Property(e => e.ManifestId).HasColumnName("ManifestID");

            entity.HasOne(d => d.Do).WithMany(p => p.LogManifestLines)
                .HasForeignKey(d => d.Doid)
                .HasConstraintName("FK__LOG_Manife__DOID__2C88998B");

            entity.HasOne(d => d.Manifest).WithMany(p => p.LogManifestLines)
                .HasForeignKey(d => d.ManifestId)
                .HasConstraintName("FK__LOG_Manif__Manif__2B947552");
        });

        modelBuilder.Entity<LogRoute>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__LOG_Rout__80979AADF5FC4ABA");

            entity.ToTable("LOG_Routes");

            entity.HasIndex(e => e.RouteCode, "UQ__LOG_Rout__FDC34585901686C9").IsUnique();

            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.RouteCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RouteName).HasMaxLength(200);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LogRoutes)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__LOG_Route__Creat__22FF2F51");
        });

        modelBuilder.Entity<LogVehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__LOG_Vehi__476B54B2D67019D1");

            entity.ToTable("LOG_Vehicles");

            entity.HasIndex(e => e.LicensePlate, "UQ__LOG_Vehi__026BC15CC12A33BD").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.Capacity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ManagedByNavigation).WithMany(p => p.LogVehicles)
                .HasForeignKey(d => d.ManagedBy)
                .HasConstraintName("FK__LOG_Vehic__Manag__1C5231C2");
        });

        modelBuilder.Entity<PurOrder>(entity =>
        {
            entity.HasKey(e => e.Poid).HasName("PK__PUR_Orde__5F02A2F47765E3B0");

            entity.ToTable("PUR_Orders");

            entity.HasIndex(e => e.Pocode, "UQ__PUR_Orde__40ACF5B896A39BD2").IsUnique();

            entity.Property(e => e.Poid).HasColumnName("POID");
            entity.Property(e => e.Pocode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("POCode");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TermId).HasColumnName("TermID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurOrders)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__PUR_Order__Suppl__6BE40491");

            entity.HasOne(d => d.Term).WithMany(p => p.PurOrders)
                .HasForeignKey(d => d.TermId)
                .HasConstraintName("FK__PUR_Order__TermI__6CD828CA");
        });

        modelBuilder.Entity<PurOrderLine>(entity =>
        {
            entity.HasKey(e => e.PolineId).HasName("PK__PUR_Orde__07B9D34206B52C3D");

            entity.ToTable("PUR_OrderLines");

            entity.Property(e => e.PolineId).HasColumnName("POLineID");
            entity.Property(e => e.Poid).HasColumnName("POID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Po).WithMany(p => p.PurOrderLines)
                .HasForeignKey(d => d.Poid)
                .HasConstraintName("FK__PUR_OrderL__POID__6FB49575");

            entity.HasOne(d => d.Variant).WithMany(p => p.PurOrderLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__PUR_Order__Varia__70A8B9AE");
        });

        modelBuilder.Entity<PurReceipt>(entity =>
        {
            entity.HasKey(e => e.Grnid).HasName("PK__PUR_Rece__BC0E8C62C5B2644F");

            entity.ToTable("PUR_Receipts");

            entity.HasIndex(e => e.Grncode, "UQ__PUR_Rece__F1E8DDCB620A3163").IsUnique();

            entity.Property(e => e.Grnid).HasColumnName("GRNID");
            entity.Property(e => e.Grncode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GRNCode");
            entity.Property(e => e.Poid).HasColumnName("POID");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Po).WithMany(p => p.PurReceipts)
                .HasForeignKey(d => d.Poid)
                .HasConstraintName("FK__PUR_Receip__POID__74794A92");

            entity.HasOne(d => d.Receiver).WithMany(p => p.PurReceipts)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__PUR_Recei__Recei__76619304");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurReceipts)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__PUR_Recei__Wareh__756D6ECB");
        });

        modelBuilder.Entity<PurReceiptLine>(entity =>
        {
            entity.HasKey(e => e.GrnlineId).HasName("PK__PUR_Rece__433F1EB0D1A586CF");

            entity.ToTable("PUR_ReceiptLines");

            entity.Property(e => e.GrnlineId).HasColumnName("GRNLineID");
            entity.Property(e => e.Grnid).HasColumnName("GRNID");
            entity.Property(e => e.PolineId).HasColumnName("POLineID");

            entity.HasOne(d => d.Grn).WithMany(p => p.PurReceiptLines)
                .HasForeignKey(d => d.Grnid)
                .HasConstraintName("FK__PUR_Recei__GRNID__793DFFAF");

            entity.HasOne(d => d.Poline).WithMany(p => p.PurReceiptLines)
                .HasForeignKey(d => d.PolineId)
                .HasConstraintName("FK__PUR_Recei__POLin__7A3223E8");
        });

        modelBuilder.Entity<PurRequest>(entity =>
        {
            entity.HasKey(e => e.Prid).HasName("PK__PUR_Requ__BC40187DADBD1FC4");

            entity.ToTable("PUR_Requests");

            entity.HasIndex(e => e.Prcode, "UQ__PUR_Requ__9E4C02EBFB5B2389").IsUnique();

            entity.Property(e => e.Prid).HasColumnName("PRID");
            entity.Property(e => e.Prcode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PRCode");
            entity.Property(e => e.RequesterId).HasColumnName("RequesterID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Requester).WithMany(p => p.PurRequests)
                .HasForeignKey(d => d.RequesterId)
                .HasConstraintName("FK__PUR_Reque__Reque__6442E2C9");
        });

        modelBuilder.Entity<PurRequestLine>(entity =>
        {
            entity.HasKey(e => e.PrlineId).HasName("PK__PUR_Requ__E959CB4CF4A1B77F");

            entity.ToTable("PUR_RequestLines");

            entity.Property(e => e.PrlineId).HasColumnName("PRLineID");
            entity.Property(e => e.Prid).HasColumnName("PRID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Pr).WithMany(p => p.PurRequestLines)
                .HasForeignKey(d => d.Prid)
                .HasConstraintName("FK__PUR_Reques__PRID__671F4F74");

            entity.HasOne(d => d.Variant).WithMany(p => p.PurRequestLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__PUR_Reque__Varia__681373AD");
        });

        modelBuilder.Entity<PurReturn>(entity =>
        {
            entity.HasKey(e => e.PreturnId).HasName("PK__PUR_Retu__1AF8A3077EAAA2AC");

            entity.ToTable("PUR_Returns");

            entity.HasIndex(e => e.ReturnCode, "UQ__PUR_Retu__4CF726C9C68FC9E9").IsUnique();

            entity.Property(e => e.PreturnId).HasColumnName("PReturnID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.Grnid).HasColumnName("GRNID");
            entity.Property(e => e.ReturnCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Creator).WithMany(p => p.PurReturns)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__PUR_Retur__Creat__7EF6D905");

            entity.HasOne(d => d.Grn).WithMany(p => p.PurReturns)
                .HasForeignKey(d => d.Grnid)
                .HasConstraintName("FK__PUR_Retur__GRNID__7E02B4CC");
        });

        modelBuilder.Entity<PurReturnLine>(entity =>
        {
            entity.HasKey(e => e.PreturnLineId).HasName("PK__PUR_Retu__5213AFA28BA0D95E");

            entity.ToTable("PUR_ReturnLines");

            entity.Property(e => e.PreturnLineId).HasColumnName("PReturnLineID");
            entity.Property(e => e.PreturnId).HasColumnName("PReturnID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Preturn).WithMany(p => p.PurReturnLines)
                .HasForeignKey(d => d.PreturnId)
                .HasConstraintName("FK__PUR_Retur__PRetu__01D345B0");

            entity.HasOne(d => d.Variant).WithMany(p => p.PurReturnLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__PUR_Retur__Varia__02C769E9");
        });

        modelBuilder.Entity<PurSupplierQuote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("PK__PUR_Supp__AF9688E18301B90D");

            entity.ToTable("PUR_SupplierQuotes");

            entity.Property(e => e.QuoteId).HasColumnName("QuoteID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PurSupplierQuotes)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__PUR_Suppl__Creat__0697FACD");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurSupplierQuotes)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__PUR_Suppl__Suppl__05A3D694");
        });

        modelBuilder.Entity<PurSupplierQuoteLine>(entity =>
        {
            entity.HasKey(e => e.QuoteLineId).HasName("PK__PUR_Supp__89C6C92BDBD33EA1");

            entity.ToTable("PUR_SupplierQuoteLines");

            entity.Property(e => e.QuoteLineId).HasColumnName("QuoteLineID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuoteId).HasColumnName("QuoteID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Quote).WithMany(p => p.PurSupplierQuoteLines)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("FK__PUR_Suppl__Quote__09746778");

            entity.HasOne(d => d.Variant).WithMany(p => p.PurSupplierQuoteLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__PUR_Suppl__Varia__0A688BB1");
        });

        modelBuilder.Entity<SalDelivery>(entity =>
        {
            entity.HasKey(e => e.Doid).HasName("PK__SAL_Deli__22F0E0FE81A3C392");

            entity.ToTable("SAL_Deliveries");

            entity.HasIndex(e => e.Docode, "UQ__SAL_Deli__33C65279AD860C60").IsUnique();

            entity.Property(e => e.Doid).HasColumnName("DOID");
            entity.Property(e => e.DispatcherId).HasColumnName("DispatcherID");
            entity.Property(e => e.Docode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DOCode");
            entity.Property(e => e.Soid).HasColumnName("SOID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Dispatcher).WithMany(p => p.SalDeliveries)
                .HasForeignKey(d => d.DispatcherId)
                .HasConstraintName("FK__SAL_Deliv__Dispa__214BF109");

            entity.HasOne(d => d.So).WithMany(p => p.SalDeliveries)
                .HasForeignKey(d => d.Soid)
                .HasConstraintName("FK__SAL_Delive__SOID__1F63A897");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SalDeliveries)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__SAL_Deliv__Wareh__2057CCD0");
        });

        modelBuilder.Entity<SalDeliveryLine>(entity =>
        {
            entity.HasKey(e => e.DolineId).HasName("PK__SAL_Deli__9D7A619BFF955B23");

            entity.ToTable("SAL_DeliveryLines");

            entity.Property(e => e.DolineId).HasColumnName("DOLineID");
            entity.Property(e => e.Doid).HasColumnName("DOID");
            entity.Property(e => e.SolineId).HasColumnName("SOLineID");

            entity.HasOne(d => d.Do).WithMany(p => p.SalDeliveryLines)
                .HasForeignKey(d => d.Doid)
                .HasConstraintName("FK__SAL_Delive__DOID__24285DB4");

            entity.HasOne(d => d.Soline).WithMany(p => p.SalDeliveryLines)
                .HasForeignKey(d => d.SolineId)
                .HasConstraintName("FK__SAL_Deliv__SOLin__251C81ED");
        });

        modelBuilder.Entity<SalOrder>(entity =>
        {
            entity.HasKey(e => e.Soid).HasName("PK__SAL_Orde__A7FF3362FEC0EF7C");

            entity.ToTable("SAL_Orders");

            entity.HasIndex(e => e.Socode, "UQ__SAL_Orde__27B177430A95713C").IsUnique();

            entity.Property(e => e.Soid).HasColumnName("SOID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Socode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SOCode");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TermId).HasColumnName("TermID");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalOrders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__SAL_Order__Custo__16CE6296");

            entity.HasOne(d => d.Term).WithMany(p => p.SalOrders)
                .HasForeignKey(d => d.TermId)
                .HasConstraintName("FK__SAL_Order__TermI__17C286CF");

            entity.HasMany(d => d.Promos).WithMany(p => p.Sos)
                .UsingEntity<Dictionary<string, object>>(
                    "SalOrderPromotion",
                    r => r.HasOne<SalPromotion>().WithMany()
                        .HasForeignKey("PromoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SAL_Order__Promo__3552E9B6"),
                    l => l.HasOne<SalOrder>().WithMany()
                        .HasForeignKey("Soid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SAL_OrderP__SOID__345EC57D"),
                    j =>
                    {
                        j.HasKey("Soid", "PromoId").HasName("PK__SAL_Orde__B4C2002F42E24B66");
                        j.ToTable("SAL_OrderPromotions");
                        j.IndexerProperty<int>("Soid").HasColumnName("SOID");
                        j.IndexerProperty<int>("PromoId").HasColumnName("PromoID");
                    });
        });

        modelBuilder.Entity<SalOrderLine>(entity =>
        {
            entity.HasKey(e => e.SolineId).HasName("PK__SAL_Orde__599D14355C1E8C5C");

            entity.ToTable("SAL_OrderLines");

            entity.Property(e => e.SolineId).HasColumnName("SOLineID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Soid).HasColumnName("SOID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.So).WithMany(p => p.SalOrderLines)
                .HasForeignKey(d => d.Soid)
                .HasConstraintName("FK__SAL_OrderL__SOID__1A9EF37A");

            entity.HasOne(d => d.Variant).WithMany(p => p.SalOrderLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__SAL_Order__Varia__1B9317B3");
        });

        modelBuilder.Entity<SalPromotion>(entity =>
        {
            entity.HasKey(e => e.PromoId).HasName("PK__SAL_Prom__33D334D0F898E184");

            entity.ToTable("SAL_Promotions");

            entity.HasIndex(e => e.PromoCode, "UQ__SAL_Prom__32DBED354F211F0C").IsUnique();

            entity.Property(e => e.PromoId).HasColumnName("PromoID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SalPromotions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__SAL_Promo__Creat__318258D2");
        });

        modelBuilder.Entity<SalQuotation>(entity =>
        {
            entity.HasKey(e => e.Sqid).HasName("PK__SAL_Quot__F4727690CA6B2262");

            entity.ToTable("SAL_Quotations");

            entity.HasIndex(e => e.Sqcode, "UQ__SAL_Quot__0CBC62E05016F503").IsUnique();

            entity.Property(e => e.Sqid).HasColumnName("SQID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Sqcode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SQCode");

            entity.HasOne(d => d.Creator).WithMany(p => p.SalQuotations)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__SAL_Quota__Creat__0F2D40CE");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalQuotations)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__SAL_Quota__Custo__0E391C95");
        });

        modelBuilder.Entity<SalQuotationLine>(entity =>
        {
            entity.HasKey(e => e.SqlineId).HasName("PK__SAL_Quot__F3300B1F966967E7");

            entity.ToTable("SAL_QuotationLines");

            entity.Property(e => e.SqlineId).HasColumnName("SQLineID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sqid).HasColumnName("SQID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Sq).WithMany(p => p.SalQuotationLines)
                .HasForeignKey(d => d.Sqid)
                .HasConstraintName("FK__SAL_Quotat__SQID__1209AD79");

            entity.HasOne(d => d.Variant).WithMany(p => p.SalQuotationLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__SAL_Quota__Varia__12FDD1B2");
        });

        modelBuilder.Entity<SalReturn>(entity =>
        {
            entity.HasKey(e => e.SreturnId).HasName("PK__SAL_Retu__549845C2A2E466C5");

            entity.ToTable("SAL_Returns");

            entity.HasIndex(e => e.ReturnCode, "UQ__SAL_Retu__4CF726C91C8F8E5B").IsUnique();

            entity.Property(e => e.SreturnId).HasColumnName("SReturnID");
            entity.Property(e => e.Doid).HasColumnName("DOID");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.ReturnCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Do).WithMany(p => p.SalReturns)
                .HasForeignKey(d => d.Doid)
                .HasConstraintName("FK__SAL_Return__DOID__28ED12D1");

            entity.HasOne(d => d.Receiver).WithMany(p => p.SalReturns)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__SAL_Retur__Recei__29E1370A");
        });

        modelBuilder.Entity<SalReturnLine>(entity =>
        {
            entity.HasKey(e => e.SreturnLineId).HasName("PK__SAL_Retu__041A7130DDCB7575");

            entity.ToTable("SAL_ReturnLines");

            entity.Property(e => e.SreturnLineId).HasColumnName("SReturnLineID");
            entity.Property(e => e.SreturnId).HasColumnName("SReturnID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Sreturn).WithMany(p => p.SalReturnLines)
                .HasForeignKey(d => d.SreturnId)
                .HasConstraintName("FK__SAL_Retur__SRetu__2CBDA3B5");

            entity.HasOne(d => d.Variant).WithMany(p => p.SalReturnLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__SAL_Retur__Varia__2DB1C7EE");
        });

        modelBuilder.Entity<SysAuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SYS_Audi__5E5499A885227A10");

            entity.ToTable("SYS_AuditLogs");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LogDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TableName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.SysAuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SYS_Audit__UserI__75A278F5");
        });

        modelBuilder.Entity<SysEmailTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__SYS_Emai__F87ADD07C38347A8");

            entity.ToTable("SYS_EmailTemplates");

            entity.HasIndex(e => e.Code, "UQ__SYS_Emai__A25C5AA79BCF86B7").IsUnique();

            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SysEmailTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__SYS_Email__Creat__72C60C4A");
        });

        modelBuilder.Entity<SysErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorId).HasName("PK__SYS_Erro__358565CA1E76050B");

            entity.ToTable("SYS_ErrorLogs");

            entity.Property(e => e.ErrorId).HasColumnName("ErrorID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.SysErrorLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SYS_Error__UserI__797309D9");
        });

        modelBuilder.Entity<SysFeature>(entity =>
        {
            entity.HasKey(e => e.FeatureId).HasName("PK__SYS_Feat__82230A2904BE3690");

            entity.ToTable("SYS_Features");

            entity.HasIndex(e => e.FeatureCode, "UQ__SYS_Feat__75CE31548B7A0957").IsUnique();

            entity.Property(e => e.FeatureId).HasColumnName("FeatureID");
            entity.Property(e => e.FeatureCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FeatureName).HasMaxLength(200);
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");

            entity.HasOne(d => d.Module).WithMany(p => p.SysFeatures)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__SYS_Featu__Modul__6754599E");
        });

        modelBuilder.Entity<SysModule>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__SYS_Modu__2B7477873DC4C1D5");

            entity.ToTable("SYS_Modules");

            entity.HasIndex(e => e.ModuleCode, "UQ__SYS_Modu__EB27D4332979CFC6").IsUnique();

            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModuleName).HasMaxLength(100);
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__SYS_Role__8AFACE3A1F083963");

            entity.ToTable("SYS_Roles");

            entity.HasIndex(e => e.RoleCode, "UQ__SYS_Role__D62CB59C4B1C64E1").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoleName).HasMaxLength(100);

            entity.HasMany(d => d.Features).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "SysRoleFeature",
                    r => r.HasOne<SysFeature>().WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SYS_RoleF__Featu__6B24EA82"),
                    l => l.HasOne<SysRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SYS_RoleF__RoleI__6A30C649"),
                    j =>
                    {
                        j.HasKey("RoleId", "FeatureId").HasName("PK__SYS_Role__02D8FE981C33B340");
                        j.ToTable("SYS_RoleFeatures");
                        j.IndexerProperty<int>("RoleId").HasColumnName("RoleID");
                        j.IndexerProperty<int>("FeatureId").HasColumnName("FeatureID");
                    });
        });

        modelBuilder.Entity<SysSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__SYS_Sett__54372AFD76C58850");

            entity.ToTable("SYS_Settings");

            entity.HasIndex(e => e.SettingKey, "UQ__SYS_Sett__01E719AD967E8DC7").IsUnique();

            entity.Property(e => e.SettingId).HasColumnName("SettingID");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SysSettings)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__SYS_Setti__Updat__6EF57B66");
        });

        modelBuilder.Entity<SysUiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SYS_UiLogs");

            entity.ToTable("SYS_UiLogs");

            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.Property(e => e.LogDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.SysUser)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SYS_UiLog__UserID");
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__SYS_User__1788CCAC729A9E3B");

            entity.ToTable("SYS_Users");

            entity.HasIndex(e => e.Username, "UQ__SYS_User__536C85E4D854B446").IsUnique();

            entity.HasIndex(e => e.EmployeeId, "UQ__SYS_User__7AD04FF06971FC95").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithOne(p => p.SysUser)
                .HasForeignKey<SysUser>(d => d.EmployeeId)
                .HasConstraintName("FK__SYS_Users__Emplo__5CD6CB2B");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "SysUserRole",
                    r => r.HasOne<SysRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SYS_UserR__RoleI__60A75C0F"),
                    l => l.HasOne<SysUser>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SYS_UserR__UserI__5FB337D6"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__SYS_User__AF27604FC0D42824");
                        j.ToTable("SYS_UserRoles");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                        j.IndexerProperty<int>("RoleId").HasColumnName("RoleID");
                    });
        });

        modelBuilder.Entity<WmsAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjId).HasName("PK__WMS_Adju__A065A852E8D04293");

            entity.ToTable("WMS_Adjustments");

            entity.HasIndex(e => e.AdjCode, "UQ__WMS_Adju__FA7B48382F1ED6D5").IsUnique();

            entity.Property(e => e.AdjId).HasColumnName("AdjID");
            entity.Property(e => e.AdjCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ApproverId).HasColumnName("ApproverID");
            entity.Property(e => e.CheckId).HasColumnName("CheckID");

            entity.HasOne(d => d.Approver).WithMany(p => p.WmsAdjustments)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("FK__WMS_Adjus__Appro__4C364F0E");

            entity.HasOne(d => d.Check).WithMany(p => p.WmsAdjustments)
                .HasForeignKey(d => d.CheckId)
                .HasConstraintName("FK__WMS_Adjus__Check__4B422AD5");
        });

        modelBuilder.Entity<WmsAdjustmentLine>(entity =>
        {
            entity.HasKey(e => e.AdjLineId).HasName("PK__WMS_Adju__4B9DA6758B19690F");

            entity.ToTable("WMS_AdjustmentLines");

            entity.Property(e => e.AdjLineId).HasColumnName("AdjLineID");
            entity.Property(e => e.AdjId).HasColumnName("AdjID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Adj).WithMany(p => p.WmsAdjustmentLines)
                .HasForeignKey(d => d.AdjId)
                .HasConstraintName("FK__WMS_Adjus__AdjID__4F12BBB9");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsAdjustmentLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_Adjus__Varia__5006DFF2");
        });

        modelBuilder.Entity<WmsDefect>(entity =>
        {
            entity.HasKey(e => e.DefectId).HasName("PK__WMS_Defe__144A37FCC34AE6C5");

            entity.ToTable("WMS_Defects");

            entity.HasIndex(e => e.DefectCode, "UQ__WMS_Defe__EB5D7BE5ADD22BDA").IsUnique();

            entity.Property(e => e.DefectId).HasColumnName("DefectID");
            entity.Property(e => e.DefectCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReporterId).HasColumnName("ReporterID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Reporter).WithMany(p => p.WmsDefects)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("FK__WMS_Defec__Repor__54CB950F");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WmsDefects)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__WMS_Defec__Wareh__53D770D6");
        });

        modelBuilder.Entity<WmsDefectLine>(entity =>
        {
            entity.HasKey(e => e.DefectLineId).HasName("PK__WMS_Defe__AE55FDD6D46EE979");

            entity.ToTable("WMS_DefectLines");

            entity.Property(e => e.DefectLineId).HasColumnName("DefectLineID");
            entity.Property(e => e.DefectId).HasColumnName("DefectID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Defect).WithMany(p => p.WmsDefectLines)
                .HasForeignKey(d => d.DefectId)
                .HasConstraintName("FK__WMS_Defec__Defec__57A801BA");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsDefectLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_Defec__Varia__589C25F3");
        });

        modelBuilder.Entity<WmsInvCheck>(entity =>
        {
            entity.HasKey(e => e.CheckId).HasName("PK__WMS_InvC__868157063AB8D338");

            entity.ToTable("WMS_InvChecks");

            entity.HasIndex(e => e.CheckCode, "UQ__WMS_InvC__3DD1954B92E2CB8E").IsUnique();

            entity.Property(e => e.CheckId).HasColumnName("CheckID");
            entity.Property(e => e.CheckCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CheckerId).HasColumnName("CheckerID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Checker).WithMany(p => p.WmsInvChecks)
                .HasForeignKey(d => d.CheckerId)
                .HasConstraintName("FK__WMS_InvCh__Check__43A1090D");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WmsInvChecks)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__WMS_InvCh__Wareh__42ACE4D4");
        });

        modelBuilder.Entity<WmsInvCheckLine>(entity =>
        {
            entity.HasKey(e => e.CheckLineId).HasName("PK__WMS_InvC__DBD1C9158865A82E");

            entity.ToTable("WMS_InvCheckLines");

            entity.Property(e => e.CheckLineId).HasColumnName("CheckLineID");
            entity.Property(e => e.CheckId).HasColumnName("CheckID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Check).WithMany(p => p.WmsInvCheckLines)
                .HasForeignKey(d => d.CheckId)
                .HasConstraintName("FK__WMS_InvCh__Check__467D75B8");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsInvCheckLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_InvCh__Varia__477199F1");
        });

        modelBuilder.Entity<WmsLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__WMS_Loca__E7FEA4773FDFB876");

            entity.ToTable("WMS_Locations");

            entity.HasIndex(e => e.LocationCode, "UQ__WMS_Loca__DDB144D5E877EC8A").IsUnique();

            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RackId).HasColumnName("RackID");

            entity.HasOne(d => d.Rack).WithMany(p => p.WmsLocations)
                .HasForeignKey(d => d.RackId)
                .HasConstraintName("FK__WMS_Locat__RackI__5D95E53A");
        });

        modelBuilder.Entity<WmsLocationType>(entity =>
        {
            entity.HasKey(e => e.LocTypeId).HasName("PK__WMS_Loca__CFA3E4534938E8F6");

            entity.ToTable("WMS_LocationTypes");

            entity.Property(e => e.LocTypeId).HasColumnName("LocTypeID");
            entity.Property(e => e.TypeName).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.WmsLocationTypes)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__WMS_Locat__Creat__607251E5");
        });

        modelBuilder.Entity<WmsRack>(entity =>
        {
            entity.HasKey(e => e.RackId).HasName("PK__WMS_Rack__0363D948EA2B1C9D");

            entity.ToTable("WMS_Racks");

            entity.Property(e => e.RackId).HasColumnName("RackID");
            entity.Property(e => e.RackCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ZoneId).HasColumnName("ZoneID");

            entity.HasOne(d => d.Zone).WithMany(p => p.WmsRacks)
                .HasForeignKey(d => d.ZoneId)
                .HasConstraintName("FK__WMS_Racks__ZoneI__59C55456");
        });

        modelBuilder.Entity<WmsStockBalance>(entity =>
        {
            entity.HasKey(e => e.StockId).HasName("PK__WMS_Stoc__2C83A9E21344FFEF");

            entity.ToTable("WMS_StockBalances");

            entity.HasIndex(e => new { e.WarehouseId, e.LocationId, e.VariantId }, "UQ_Stock").IsUnique();

            entity.Property(e => e.StockId).HasColumnName("StockID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Location).WithMany(p => p.WmsStockBalances)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__WMS_Stock__Locat__5D60DB10");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsStockBalances)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_Stock__Varia__5E54FF49");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WmsStockBalances)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__WMS_Stock__Wareh__5C6CB6D7");
        });

        modelBuilder.Entity<WmsStockLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId).HasName("PK__WMS_Stoc__AE70E0AF9AA232DB");

            entity.ToTable("WMS_StockLedger");

            entity.Property(e => e.LedgerId).HasColumnName("LedgerID");
            entity.Property(e => e.RefCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.WmsStockLedgers)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__WMS_Stock__Creat__6319B466");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsStockLedgers)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_Stock__Varia__6225902D");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WmsStockLedgers)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__WMS_Stock__Wareh__61316BF4");
        });

        modelBuilder.Entity<WmsTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("PK__WMS_Tran__9549017176BED8C5");

            entity.ToTable("WMS_Transfers");

            entity.HasIndex(e => e.TransferCode, "UQ__WMS_Tran__CE99A4C5FA478317").IsUnique();

            entity.Property(e => e.TransferId).HasColumnName("TransferID");
            entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
            entity.Property(e => e.FromWh).HasColumnName("FromWH");
            entity.Property(e => e.ToWh).HasColumnName("ToWH");
            entity.Property(e => e.TransferCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Creator).WithMany(p => p.WmsTransfers)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("FK__WMS_Trans__Creat__3B0BC30C");

            entity.HasOne(d => d.FromWhNavigation).WithMany(p => p.WmsTransferFromWhNavigations)
                .HasForeignKey(d => d.FromWh)
                .HasConstraintName("FK__WMS_Trans__FromW__39237A9A");

            entity.HasOne(d => d.ToWhNavigation).WithMany(p => p.WmsTransferToWhNavigations)
                .HasForeignKey(d => d.ToWh)
                .HasConstraintName("FK__WMS_Transf__ToWH__3A179ED3");
        });

        modelBuilder.Entity<WmsTransferLine>(entity =>
        {
            entity.HasKey(e => e.TransLineId).HasName("PK__WMS_Tran__67BAE7AD9E258FDB");

            entity.ToTable("WMS_TransferLines");

            entity.Property(e => e.TransLineId).HasColumnName("TransLineID");
            entity.Property(e => e.TransferId).HasColumnName("TransferID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");

            entity.HasOne(d => d.Transfer).WithMany(p => p.WmsTransferLines)
                .HasForeignKey(d => d.TransferId)
                .HasConstraintName("FK__WMS_Trans__Trans__3DE82FB7");

            entity.HasOne(d => d.Variant).WithMany(p => p.WmsTransferLines)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__WMS_Trans__Varia__3EDC53F0");
        });

        modelBuilder.Entity<WmsWarehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__WMS_Ware__2608AFD97D036669");

            entity.ToTable("WMS_Warehouses");

            entity.HasIndex(e => e.Whcode, "UQ__WMS_Ware__3EB0FEFDEFE40730").IsUnique();

            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            entity.Property(e => e.BranchId).HasColumnName("BranchID");
            entity.Property(e => e.Whcode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("WHCode");
            entity.Property(e => e.Whname)
                .HasMaxLength(200)
                .HasColumnName("WHName");

            entity.HasOne(d => d.Branch).WithMany(p => p.WmsWarehouses)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__WMS_Wareh__Branc__540C7B00");
        });

        modelBuilder.Entity<WmsZone>(entity =>
        {
            entity.HasKey(e => e.ZoneId).HasName("PK__WMS_Zone__60166795CE5F26F8");

            entity.ToTable("WMS_Zones");

            entity.Property(e => e.ZoneId).HasColumnName("ZoneID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            entity.Property(e => e.ZoneCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WmsZones)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__WMS_Zones__Wareh__56E8E7AB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}