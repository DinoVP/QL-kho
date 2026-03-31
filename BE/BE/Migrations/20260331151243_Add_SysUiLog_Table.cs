using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE.Migrations
{
    /// <inheritdoc />
    public partial class Add_SysUiLog_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "FIN_ChartOfAccounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Char__349DA5860168F545", x => x.AccountID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_PaymentTerms",
                columns: table => new
                {
                    TermID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DaysDue = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Paym__410A2E456A6B7436", x => x.TermID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_TaxRates",
                columns: table => new
                {
                    TaxID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_TaxR__711BE08CE2093224", x => x.TaxID);
                });

            migrationBuilder.CreateTable(
                name: "GEO_Countries",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GEO_Coun__10D160BF10F4479C", x => x.CountryID);
                });

            migrationBuilder.CreateTable(
                name: "HRM_JobTitles",
                columns: table => new
                {
                    TitleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TitleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_JobT__757589E645F91538", x => x.TitleID);
                });

            migrationBuilder.CreateTable(
                name: "HRM_Shifts",
                columns: table => new
                {
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Shif__C0A838E1A6E7FAA4", x => x.ShiftID);
                });

            migrationBuilder.CreateTable(
                name: "ITM_Brands",
                columns: table => new
                {
                    BrandID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Bran__DAD4F3BE4D6EE7C0", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "ITM_Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CatName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Cate__19093A2B3F732579", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ITM_UoMGroups",
                columns: table => new
                {
                    UoMGroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BaseUoM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_UoMG__E5F8A20C261AF6A3", x => x.UoMGroupID);
                });

            migrationBuilder.CreateTable(
                name: "SYS_Modules",
                columns: table => new
                {
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Modu__2B7477873DC4C1D5", x => x.ModuleID);
                });

            migrationBuilder.CreateTable(
                name: "SYS_Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Role__8AFACE3A1F083963", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "GEO_Provinces",
                columns: table => new
                {
                    ProvinceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    ProvinceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GEO_Prov__FD0A6FA3A993363C", x => x.ProvinceID);
                    table.ForeignKey(
                        name: "FK__GEO_Provi__Count__3A81B327",
                        column: x => x.CountryID,
                        principalTable: "GEO_Countries",
                        principalColumn: "CountryID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_SubCategories",
                columns: table => new
                {
                    SubCatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    SubCatName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_SubC__3963797575A02B0A", x => x.SubCatID);
                    table.ForeignKey(
                        name: "FK__ITM_SubCa__Categ__25518C17",
                        column: x => x.CategoryID,
                        principalTable: "ITM_Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_UoMConversions",
                columns: table => new
                {
                    ConvID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UoMGroupID = table.Column<int>(type: "int", nullable: true),
                    AltUoM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConvRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_UoMC__F3B6120B96F06137", x => x.ConvID);
                    table.ForeignKey(
                        name: "FK__ITM_UoMCo__UoMGr__2CF2ADDF",
                        column: x => x.UoMGroupID,
                        principalTable: "ITM_UoMGroups",
                        principalColumn: "UoMGroupID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_Features",
                columns: table => new
                {
                    FeatureID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleID = table.Column<int>(type: "int", nullable: true),
                    FeatureCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    FeatureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Feat__82230A2904BE3690", x => x.FeatureID);
                    table.ForeignKey(
                        name: "FK__SYS_Featu__Modul__6754599E",
                        column: x => x.ModuleID,
                        principalTable: "SYS_Modules",
                        principalColumn: "ModuleID");
                });

            migrationBuilder.CreateTable(
                name: "GEO_Districts",
                columns: table => new
                {
                    DistrictID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceID = table.Column<int>(type: "int", nullable: true),
                    DistrictName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GEO_Dist__85FDA4A687B059BA", x => x.DistrictID);
                    table.ForeignKey(
                        name: "FK__GEO_Distr__Provi__3D5E1FD2",
                        column: x => x.ProvinceID,
                        principalTable: "GEO_Provinces",
                        principalColumn: "ProvinceID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCatID = table.Column<int>(type: "int", nullable: true),
                    BrandID = table.Column<int>(type: "int", nullable: true),
                    UoMGroupID = table.Column<int>(type: "int", nullable: true),
                    TaxID = table.Column<int>(type: "int", nullable: true),
                    SKU = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Prod__B40CC6ED666D63E6", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK__ITM_Produ__Brand__31B762FC",
                        column: x => x.BrandID,
                        principalTable: "ITM_Brands",
                        principalColumn: "BrandID");
                    table.ForeignKey(
                        name: "FK__ITM_Produ__SubCa__30C33EC3",
                        column: x => x.SubCatID,
                        principalTable: "ITM_SubCategories",
                        principalColumn: "SubCatID");
                    table.ForeignKey(
                        name: "FK__ITM_Produ__TaxID__339FAB6E",
                        column: x => x.TaxID,
                        principalTable: "FIN_TaxRates",
                        principalColumn: "TaxID");
                    table.ForeignKey(
                        name: "FK__ITM_Produ__UoMGr__32AB8735",
                        column: x => x.UoMGroupID,
                        principalTable: "ITM_UoMGroups",
                        principalColumn: "UoMGroupID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_RoleFeatures",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    FeatureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Role__02D8FE981C33B340", x => new { x.RoleID, x.FeatureID });
                    table.ForeignKey(
                        name: "FK__SYS_RoleF__Featu__6B24EA82",
                        column: x => x.FeatureID,
                        principalTable: "SYS_Features",
                        principalColumn: "FeatureID");
                    table.ForeignKey(
                        name: "FK__SYS_RoleF__RoleI__6A30C649",
                        column: x => x.RoleID,
                        principalTable: "SYS_Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Images",
                columns: table => new
                {
                    ImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Imag__7516F4ECDCC9837E", x => x.ImageID);
                    table.ForeignKey(
                        name: "FK__ITM_Image__Produ__3E1D39E1",
                        column: x => x.ProductID,
                        principalTable: "ITM_Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Variants",
                columns: table => new
                {
                    VariantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    VariantSKU = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Vari__0EA233E43D9184DE", x => x.VariantID);
                    table.ForeignKey(
                        name: "FK__ITM_Varia__Produ__37703C52",
                        column: x => x.ProductID,
                        principalTable: "ITM_Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Barcodes",
                columns: table => new
                {
                    BarcodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    Barcode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Barc__21916C88414D95B5", x => x.BarcodeID);
                    table.ForeignKey(
                        name: "FK__ITM_Barco__Varia__3B40CD36",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Serials",
                columns: table => new
                {
                    SerialID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    SerialNo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Seri__5E5B3EC4B26D29D0", x => x.SerialID);
                    table.ForeignKey(
                        name: "FK__ITM_Seria__Varia__498EEC8D",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "CRM_Addresses",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: true),
                    DistrictID = table.Column<int>(type: "int", nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Addr__091C2A1BC2A12ECA", x => x.AddressID);
                    table.ForeignKey(
                        name: "FK__CRM_Addre__Distr__08B54D69",
                        column: x => x.DistrictID,
                        principalTable: "GEO_Districts",
                        principalColumn: "DistrictID");
                });

            migrationBuilder.CreateTable(
                name: "CRM_BankAccounts",
                columns: table => new
                {
                    BankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Bank__AA08CB33FB5ED6D6", x => x.BankID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_Contacts",
                columns: table => new
                {
                    ContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Cont__5C6625BB3D1091CC", x => x.ContactID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_Contracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: true),
                    ContractNo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Cont__C90D3409730F598C", x => x.ContractID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_CustomerClasses",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Cust__CB1927A09B86E037", x => x.ClassID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_PartnerGroups",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Part__149AF30A0EC8D356", x => x.GroupID);
                });

            migrationBuilder.CreateTable(
                name: "CRM_Partners",
                columns: table => new
                {
                    PartnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupID = table.Column<int>(type: "int", nullable: true),
                    PartnerCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsSupplier = table.Column<bool>(type: "bit", nullable: true),
                    IsCustomer = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Part__39FD6332AEDA337F", x => x.PartnerID);
                    table.ForeignKey(
                        name: "FK__CRM_Partn__Group__04E4BC85",
                        column: x => x.GroupID,
                        principalTable: "CRM_PartnerGroups",
                        principalColumn: "GroupID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Batches",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    BatchNo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Batc__5D55CE3894C6E6DD", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK__ITM_Batch__Suppl__45BE5BA9",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                    table.ForeignKey(
                        name: "FK__ITM_Batch__Varia__44CA3770",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_Orders",
                columns: table => new
                {
                    POID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Orde__5F02A2F47765E3B0", x => x.POID);
                    table.ForeignKey(
                        name: "FK__PUR_Order__Suppl__6BE40491",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                    table.ForeignKey(
                        name: "FK__PUR_Order__TermI__6CD828CA",
                        column: x => x.TermID,
                        principalTable: "FIN_PaymentTerms",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_Orders",
                columns: table => new
                {
                    SOID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SOCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Orde__A7FF3362FEC0EF7C", x => x.SOID);
                    table.ForeignKey(
                        name: "FK__SAL_Order__Custo__16CE6296",
                        column: x => x.CustomerID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                    table.ForeignKey(
                        name: "FK__SAL_Order__TermI__17C286CF",
                        column: x => x.TermID,
                        principalTable: "FIN_PaymentTerms",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_OrderLines",
                columns: table => new
                {
                    POLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    OrderQty = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Orde__07B9D34206B52C3D", x => x.POLineID);
                    table.ForeignKey(
                        name: "FK__PUR_OrderL__POID__6FB49575",
                        column: x => x.POID,
                        principalTable: "PUR_Orders",
                        principalColumn: "POID");
                    table.ForeignKey(
                        name: "FK__PUR_Order__Varia__70A8B9AE",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_OrderLines",
                columns: table => new
                {
                    SOLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SOID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    OrderQty = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Orde__599D14355C1E8C5C", x => x.SOLineID);
                    table.ForeignKey(
                        name: "FK__SAL_OrderL__SOID__1A9EF37A",
                        column: x => x.SOID,
                        principalTable: "SAL_Orders",
                        principalColumn: "SOID");
                    table.ForeignKey(
                        name: "FK__SAL_Order__Varia__1B9317B3",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "CRM_PriceLists",
                columns: table => new
                {
                    PriceListID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Pric__1E30F34C48BB0AAE", x => x.PriceListID);
                });

            migrationBuilder.CreateTable(
                name: "ITM_PriceListDetails",
                columns: table => new
                {
                    PriceListID = table.Column<int>(type: "int", nullable: false),
                    VariantID = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Pric__4EDAD072E33280D3", x => new { x.PriceListID, x.VariantID });
                    table.ForeignKey(
                        name: "FK__ITM_Price__Price__40F9A68C",
                        column: x => x.PriceListID,
                        principalTable: "CRM_PriceLists",
                        principalColumn: "PriceListID");
                    table.ForeignKey(
                        name: "FK__ITM_Price__Varia__41EDCAC5",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "CRM_SupplierEvals",
                columns: table => new
                {
                    EvalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    EvaluatorID = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CRM_Supp__C1A298ADDFD6C296", x => x.EvalID);
                    table.ForeignKey(
                        name: "FK__CRM_Suppl__Suppl__114A936A",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "FIN_AP_InvoiceLines",
                columns: table => new
                {
                    APLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APInvID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_AP_I__3AFB7CDB9B17A966", x => x.APLineID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_AP_Invoices",
                columns: table => new
                {
                    APInvID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    GRNID = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_AP_I__A3A91459494E1DD7", x => x.APInvID);
                    table.ForeignKey(
                        name: "FK__FIN_AP_In__Suppl__7BE56230",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "FIN_AR_InvoiceLines",
                columns: table => new
                {
                    ARLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ARInvID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_AR_I__A7CE4CBB370518DB", x => x.ARLineID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_AR_Invoices",
                columns: table => new
                {
                    ARInvID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    DOID = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_AR_I__BFD2EBB5AA37E360", x => x.ARInvID);
                    table.ForeignKey(
                        name: "FK__FIN_AR_In__Custo__02925FBF",
                        column: x => x.CustomerID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "FIN_CostCenters",
                columns: table => new
                {
                    CenterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Cost__398FC7D7E47FC390", x => x.CenterID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_Depreciations",
                columns: table => new
                {
                    DepID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: true),
                    DepAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Depr__DB9CAA7F2E3FF781", x => x.DepID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_FiscalPeriods",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: true),
                    ClosedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Fisc__E521BB3627EFCA1C", x => x.PeriodID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_FixedAssets",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecordedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Fixe__43492372B61FBC6E", x => x.AssetID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_JournalLines",
                columns: table => new
                {
                    LineID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalID = table.Column<long>(type: "bigint", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Jour__2EAE64C9696C8EF6", x => x.LineID);
                    table.ForeignKey(
                        name: "FK__FIN_Journ__Accou__74444068",
                        column: x => x.AccountID,
                        principalTable: "FIN_ChartOfAccounts",
                        principalColumn: "AccountID");
                });

            migrationBuilder.CreateTable(
                name: "FIN_Journals",
                columns: table => new
                {
                    JournalID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Jour__250103868F212644", x => x.JournalID);
                });

            migrationBuilder.CreateTable(
                name: "FIN_Payments",
                columns: table => new
                {
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    CashID = table.Column<int>(type: "int", nullable: true),
                    BankAccID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Paym__9B556A5861479487", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK__FIN_Payme__BankA__0B27A5C0",
                        column: x => x.BankAccID,
                        principalTable: "FIN_BankAccounts",
                        principalColumn: "BankAccID");
                    table.ForeignKey(
                        name: "FK__FIN_Payme__CashI__0A338187",
                        column: x => x.CashID,
                        principalTable: "FIN_CashAccounts",
                        principalColumn: "CashID");
                    table.ForeignKey(
                        name: "FK__FIN_Payme__Suppl__093F5D4E",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "FIN_Receipts",
                columns: table => new
                {
                    ReceiptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    CashID = table.Column<int>(type: "int", nullable: true),
                    BankAccID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FIN_Rece__CC08C400C99D6CAA", x => x.ReceiptID);
                    table.ForeignKey(
                        name: "FK__FIN_Recei__BankA__10E07F16",
                        column: x => x.BankAccID,
                        principalTable: "FIN_BankAccounts",
                        principalColumn: "BankAccID");
                    table.ForeignKey(
                        name: "FK__FIN_Recei__CashI__0FEC5ADD",
                        column: x => x.CashID,
                        principalTable: "FIN_CashAccounts",
                        principalColumn: "CashID");
                    table.ForeignKey(
                        name: "FK__FIN_Recei__Custo__0EF836A4",
                        column: x => x.CustomerID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "HRM_Branches",
                columns: table => new
                {
                    BranchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictID = table.Column<int>(type: "int", nullable: true),
                    BranchCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    WarehouseCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Bran__A1682FA589BF5E3E", x => x.BranchID);
                    table.ForeignKey(
                        name: "FK__HRM_Branc__Distr__412EB0B6",
                        column: x => x.DistrictID,
                        principalTable: "GEO_Districts",
                        principalColumn: "DistrictID");
                });

            migrationBuilder.CreateTable(
                name: "HRM_Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    DeptCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DeptName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Depa__B2079BCD73AFAD23", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK__HRM_Depar__Branc__44FF419A",
                        column: x => x.BranchID,
                        principalTable: "HRM_Branches",
                        principalColumn: "BranchID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Warehouses",
                columns: table => new
                {
                    WarehouseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    WHCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    WHName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WhAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Ware__2608AFD97D036669", x => x.WarehouseID);
                    table.ForeignKey(
                        name: "FK__WMS_Wareh__Branc__540C7B00",
                        column: x => x.BranchID,
                        principalTable: "HRM_Branches",
                        principalColumn: "BranchID");
                });

            migrationBuilder.CreateTable(
                name: "HRM_Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    TitleID = table.Column<int>(type: "int", nullable: true),
                    EmpCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Empl__7AD04FF137B10A91", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_HRM_Employees_HRM_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "HRM_Branches",
                        principalColumn: "BranchID");
                    table.ForeignKey(
                        name: "FK_HRM_Employees_WMS_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                    table.ForeignKey(
                        name: "FK__HRM_Emplo__Depar__4BAC3F29",
                        column: x => x.DepartmentID,
                        principalTable: "HRM_Departments",
                        principalColumn: "DepartmentID");
                    table.ForeignKey(
                        name: "FK__HRM_Emplo__Title__4CA06362",
                        column: x => x.TitleID,
                        principalTable: "HRM_JobTitles",
                        principalColumn: "TitleID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Zones",
                columns: table => new
                {
                    ZoneID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    ZoneCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Zone__60166795CE5F26F8", x => x.ZoneID);
                    table.ForeignKey(
                        name: "FK__WMS_Zones__Wareh__56E8E7AB",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "HRM_Payroll",
                columns: table => new
                {
                    PayrollID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    SalaryMonth = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Payr__99DFC6924102B046", x => x.PayrollID);
                    table.ForeignKey(
                        name: "FK__HRM_Payro__Emplo__5535A963",
                        column: x => x.EmployeeID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "HRM_Timesheets",
                columns: table => new
                {
                    TimesheetID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    ShiftID = table.Column<int>(type: "int", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HRM_Time__848CBECD25C3AE1A", x => x.TimesheetID);
                    table.ForeignKey(
                        name: "FK__HRM_Times__Emplo__5165187F",
                        column: x => x.EmployeeID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK__HRM_Times__Shift__52593CB8",
                        column: x => x.ShiftID,
                        principalTable: "HRM_Shifts",
                        principalColumn: "ShiftID");
                });

            migrationBuilder.CreateTable(
                name: "LOG_Drivers",
                columns: table => new
                {
                    DriverID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    LicenseClass = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOG_Driv__F1B1CD247D58B636", x => x.DriverID);
                    table.ForeignKey(
                        name: "FK__LOG_Drive__Emplo__1F2E9E6D",
                        column: x => x.EmployeeID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_Requests",
                columns: table => new
                {
                    PRID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RequesterID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Requ__BC40187DADBD1FC4", x => x.PRID);
                    table.ForeignKey(
                        name: "FK__PUR_Reque__Reque__6442E2C9",
                        column: x => x.RequesterID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_User__1788CCAC729A9E3B", x => x.UserID);
                    table.ForeignKey(
                        name: "FK__SYS_Users__Emplo__5CD6CB2B",
                        column: x => x.EmployeeID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_InvChecks",
                columns: table => new
                {
                    CheckID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    CheckerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_InvC__868157063AB8D338", x => x.CheckID);
                    table.ForeignKey(
                        name: "FK__WMS_InvCh__Check__43A1090D",
                        column: x => x.CheckerID,
                        principalTable: "HRM_Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK__WMS_InvCh__Wareh__42ACE4D4",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Racks",
                columns: table => new
                {
                    RackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneID = table.Column<int>(type: "int", nullable: true),
                    RackCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Rack__0363D948EA2B1C9D", x => x.RackID);
                    table.ForeignKey(
                        name: "FK__WMS_Racks__ZoneI__59C55456",
                        column: x => x.ZoneID,
                        principalTable: "WMS_Zones",
                        principalColumn: "ZoneID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_RequestLines",
                columns: table => new
                {
                    PRLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    ReqQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Requ__E959CB4CF4A1B77F", x => x.PRLineID);
                    table.ForeignKey(
                        name: "FK__PUR_Reque__Varia__681373AD",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                    table.ForeignKey(
                        name: "FK__PUR_Reques__PRID__671F4F74",
                        column: x => x.PRID,
                        principalTable: "PUR_Requests",
                        principalColumn: "PRID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_Attributes",
                columns: table => new
                {
                    AttrID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttrName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Attr__0108336FEA12A4B9", x => x.AttrID);
                    table.ForeignKey(
                        name: "FK__ITM_Attri__Creat__4C6B5938",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "LOG_Routes",
                columns: table => new
                {
                    RouteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RouteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOG_Rout__80979AADF5FC4ABA", x => x.RouteID);
                    table.ForeignKey(
                        name: "FK__LOG_Route__Creat__22FF2F51",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "LOG_Vehicles",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Capacity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ManagedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOG_Vehi__476B54B2D67019D1", x => x.VehicleID);
                    table.ForeignKey(
                        name: "FK__LOG_Vehic__Manag__1C5231C2",
                        column: x => x.ManagedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_Receipts",
                columns: table => new
                {
                    GRNID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    POID = table.Column<int>(type: "int", nullable: true),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    ReceiverID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Rece__BC0E8C62C5B2644F", x => x.GRNID);
                    table.ForeignKey(
                        name: "FK__PUR_Recei__Recei__76619304",
                        column: x => x.ReceiverID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__PUR_Recei__Wareh__756D6ECB",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                    table.ForeignKey(
                        name: "FK__PUR_Receip__POID__74794A92",
                        column: x => x.POID,
                        principalTable: "PUR_Orders",
                        principalColumn: "POID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_SupplierQuotes",
                columns: table => new
                {
                    QuoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    QuoteDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Supp__AF9688E18301B90D", x => x.QuoteID);
                    table.ForeignKey(
                        name: "FK__PUR_Suppl__Creat__0697FACD",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__PUR_Suppl__Suppl__05A3D694",
                        column: x => x.SupplierID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_Deliveries",
                columns: table => new
                {
                    DOID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DOCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SOID = table.Column<int>(type: "int", nullable: true),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    DispatcherID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Deli__22F0E0FE81A3C392", x => x.DOID);
                    table.ForeignKey(
                        name: "FK__SAL_Deliv__Dispa__214BF109",
                        column: x => x.DispatcherID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__SAL_Deliv__Wareh__2057CCD0",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                    table.ForeignKey(
                        name: "FK__SAL_Delive__SOID__1F63A897",
                        column: x => x.SOID,
                        principalTable: "SAL_Orders",
                        principalColumn: "SOID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_Promotions",
                columns: table => new
                {
                    PromoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Prom__33D334D0F898E184", x => x.PromoID);
                    table.ForeignKey(
                        name: "FK__SAL_Promo__Creat__318258D2",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_Quotations",
                columns: table => new
                {
                    SQID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SQCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Quot__F4727690CA6B2262", x => x.SQID);
                    table.ForeignKey(
                        name: "FK__SAL_Quota__Creat__0F2D40CE",
                        column: x => x.CreatorID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__SAL_Quota__Custo__0E391C95",
                        column: x => x.CustomerID,
                        principalTable: "CRM_Partners",
                        principalColumn: "PartnerID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_AuditLogs",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(255)", unicode: false, maxLength: 20, nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(500)", unicode: false, maxLength: 50, nullable: true),
                    LogDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Audi__5E5499A885227A10", x => x.LogID);
                    table.ForeignKey(
                        name: "FK__SYS_Audit__UserI__75A278F5",
                        column: x => x.UserID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_EmailTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Emai__F87ADD07C38347A8", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK__SYS_Email__Creat__72C60C4A",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_ErrorLogs",
                columns: table => new
                {
                    ErrorID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    ErrorMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Erro__358565CA1E76050B", x => x.ErrorID);
                    table.ForeignKey(
                        name: "FK__SYS_Error__UserI__797309D9",
                        column: x => x.UserID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_Settings",
                columns: table => new
                {
                    SettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_Sett__54372AFD76C58850", x => x.SettingID);
                    table.ForeignKey(
                        name: "FK__SYS_Setti__Updat__6EF57B66",
                        column: x => x.UpdatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_UiLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYS_UiLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK__SYS_UiLog__UserID",
                        column: x => x.UserId,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SYS_UserRoles",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SYS_User__AF27604FC0D42824", x => new { x.UserID, x.RoleID });
                    table.ForeignKey(
                        name: "FK__SYS_UserR__RoleI__60A75C0F",
                        column: x => x.RoleID,
                        principalTable: "SYS_Roles",
                        principalColumn: "RoleID");
                    table.ForeignKey(
                        name: "FK__SYS_UserR__UserI__5FB337D6",
                        column: x => x.UserID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Defects",
                columns: table => new
                {
                    DefectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefectCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    ReporterID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Defe__144A37FCC34AE6C5", x => x.DefectID);
                    table.ForeignKey(
                        name: "FK__WMS_Defec__Repor__54CB950F",
                        column: x => x.ReporterID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__WMS_Defec__Wareh__53D770D6",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_LocationTypes",
                columns: table => new
                {
                    LocTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Loca__CFA3E4534938E8F6", x => x.LocTypeID);
                    table.ForeignKey(
                        name: "FK__WMS_Locat__Creat__607251E5",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_StockLedger",
                columns: table => new
                {
                    LedgerID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    RefCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    QtyChange = table.Column<int>(type: "int", nullable: true),
                    BalanceAfter = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Stoc__AE70E0AF9AA232DB", x => x.LedgerID);
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Creat__6319B466",
                        column: x => x.CreatedBy,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Varia__6225902D",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Wareh__61316BF4",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Transfers",
                columns: table => new
                {
                    TransferID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FromWH = table.Column<int>(type: "int", nullable: true),
                    ToWH = table.Column<int>(type: "int", nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Tran__9549017176BED8C5", x => x.TransferID);
                    table.ForeignKey(
                        name: "FK__WMS_Trans__Creat__3B0BC30C",
                        column: x => x.CreatorID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__WMS_Trans__FromW__39237A9A",
                        column: x => x.FromWH,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                    table.ForeignKey(
                        name: "FK__WMS_Transf__ToWH__3A179ED3",
                        column: x => x.ToWH,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Adjustments",
                columns: table => new
                {
                    AdjID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CheckID = table.Column<int>(type: "int", nullable: true),
                    ApproverID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Adju__A065A852E8D04293", x => x.AdjID);
                    table.ForeignKey(
                        name: "FK__WMS_Adjus__Appro__4C364F0E",
                        column: x => x.ApproverID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__WMS_Adjus__Check__4B422AD5",
                        column: x => x.CheckID,
                        principalTable: "WMS_InvChecks",
                        principalColumn: "CheckID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_InvCheckLines",
                columns: table => new
                {
                    CheckLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    SystemQty = table.Column<int>(type: "int", nullable: true),
                    ActualQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_InvC__DBD1C9158865A82E", x => x.CheckLineID);
                    table.ForeignKey(
                        name: "FK__WMS_InvCh__Check__467D75B8",
                        column: x => x.CheckID,
                        principalTable: "WMS_InvChecks",
                        principalColumn: "CheckID");
                    table.ForeignKey(
                        name: "FK__WMS_InvCh__Varia__477199F1",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_Locations",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RackID = table.Column<int>(type: "int", nullable: true),
                    LocationCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Loca__E7FEA4773FDFB876", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK__WMS_Locat__RackI__5D95E53A",
                        column: x => x.RackID,
                        principalTable: "WMS_Racks",
                        principalColumn: "RackID");
                });

            migrationBuilder.CreateTable(
                name: "ITM_VariantAttributes",
                columns: table => new
                {
                    VariantID = table.Column<int>(type: "int", nullable: false),
                    AttrID = table.Column<int>(type: "int", nullable: false),
                    AttrValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ITM_Vari__EEB2B0D23A6D993F", x => new { x.VariantID, x.AttrID });
                    table.ForeignKey(
                        name: "FK__ITM_Varia__AttrI__503BEA1C",
                        column: x => x.AttrID,
                        principalTable: "ITM_Attributes",
                        principalColumn: "AttrID");
                    table.ForeignKey(
                        name: "FK__ITM_Varia__Varia__4F47C5E3",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "LOG_Manifests",
                columns: table => new
                {
                    ManifestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleID = table.Column<int>(type: "int", nullable: true),
                    DriverID = table.Column<int>(type: "int", nullable: true),
                    RouteID = table.Column<int>(type: "int", nullable: true),
                    DispatcherID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOG_Mani__6064F346D109ACEC", x => x.ManifestID);
                    table.ForeignKey(
                        name: "FK__LOG_Manif__Dispa__28B808A7",
                        column: x => x.DispatcherID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__LOG_Manif__Drive__26CFC035",
                        column: x => x.DriverID,
                        principalTable: "LOG_Drivers",
                        principalColumn: "DriverID");
                    table.ForeignKey(
                        name: "FK__LOG_Manif__Route__27C3E46E",
                        column: x => x.RouteID,
                        principalTable: "LOG_Routes",
                        principalColumn: "RouteID");
                    table.ForeignKey(
                        name: "FK__LOG_Manif__Vehic__25DB9BFC",
                        column: x => x.VehicleID,
                        principalTable: "LOG_Vehicles",
                        principalColumn: "VehicleID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_ReceiptLines",
                columns: table => new
                {
                    GRNLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNID = table.Column<int>(type: "int", nullable: true),
                    POLineID = table.Column<int>(type: "int", nullable: true),
                    ReceivedQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Rece__433F1EB0D1A586CF", x => x.GRNLineID);
                    table.ForeignKey(
                        name: "FK__PUR_Recei__GRNID__793DFFAF",
                        column: x => x.GRNID,
                        principalTable: "PUR_Receipts",
                        principalColumn: "GRNID");
                    table.ForeignKey(
                        name: "FK__PUR_Recei__POLin__7A3223E8",
                        column: x => x.POLineID,
                        principalTable: "PUR_OrderLines",
                        principalColumn: "POLineID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_Returns",
                columns: table => new
                {
                    PReturnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    GRNID = table.Column<int>(type: "int", nullable: true),
                    CreatorID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Retu__1AF8A3077EAAA2AC", x => x.PReturnID);
                    table.ForeignKey(
                        name: "FK__PUR_Retur__Creat__7EF6D905",
                        column: x => x.CreatorID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__PUR_Retur__GRNID__7E02B4CC",
                        column: x => x.GRNID,
                        principalTable: "PUR_Receipts",
                        principalColumn: "GRNID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_SupplierQuoteLines",
                columns: table => new
                {
                    QuoteLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Supp__89C6C92BDBD33EA1", x => x.QuoteLineID);
                    table.ForeignKey(
                        name: "FK__PUR_Suppl__Quote__09746778",
                        column: x => x.QuoteID,
                        principalTable: "PUR_SupplierQuotes",
                        principalColumn: "QuoteID");
                    table.ForeignKey(
                        name: "FK__PUR_Suppl__Varia__0A688BB1",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_DeliveryLines",
                columns: table => new
                {
                    DOLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DOID = table.Column<int>(type: "int", nullable: true),
                    SOLineID = table.Column<int>(type: "int", nullable: true),
                    DeliveredQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Deli__9D7A619BFF955B23", x => x.DOLineID);
                    table.ForeignKey(
                        name: "FK__SAL_Deliv__SOLin__251C81ED",
                        column: x => x.SOLineID,
                        principalTable: "SAL_OrderLines",
                        principalColumn: "SOLineID");
                    table.ForeignKey(
                        name: "FK__SAL_Delive__DOID__24285DB4",
                        column: x => x.DOID,
                        principalTable: "SAL_Deliveries",
                        principalColumn: "DOID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_Returns",
                columns: table => new
                {
                    SReturnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DOID = table.Column<int>(type: "int", nullable: true),
                    ReceiverID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Retu__549845C2A2E466C5", x => x.SReturnID);
                    table.ForeignKey(
                        name: "FK__SAL_Retur__Recei__29E1370A",
                        column: x => x.ReceiverID,
                        principalTable: "SYS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__SAL_Return__DOID__28ED12D1",
                        column: x => x.DOID,
                        principalTable: "SAL_Deliveries",
                        principalColumn: "DOID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_OrderPromotions",
                columns: table => new
                {
                    SOID = table.Column<int>(type: "int", nullable: false),
                    PromoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Orde__B4C2002F42E24B66", x => new { x.SOID, x.PromoID });
                    table.ForeignKey(
                        name: "FK__SAL_OrderP__SOID__345EC57D",
                        column: x => x.SOID,
                        principalTable: "SAL_Orders",
                        principalColumn: "SOID");
                    table.ForeignKey(
                        name: "FK__SAL_Order__Promo__3552E9B6",
                        column: x => x.PromoID,
                        principalTable: "SAL_Promotions",
                        principalColumn: "PromoID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_QuotationLines",
                columns: table => new
                {
                    SQLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SQID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Quot__F3300B1F966967E7", x => x.SQLineID);
                    table.ForeignKey(
                        name: "FK__SAL_Quota__Varia__12FDD1B2",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                    table.ForeignKey(
                        name: "FK__SAL_Quotat__SQID__1209AD79",
                        column: x => x.SQID,
                        principalTable: "SAL_Quotations",
                        principalColumn: "SQID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_DefectLines",
                columns: table => new
                {
                    DefectLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefectID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    DefectQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Defe__AE55FDD6D46EE979", x => x.DefectLineID);
                    table.ForeignKey(
                        name: "FK__WMS_Defec__Defec__57A801BA",
                        column: x => x.DefectID,
                        principalTable: "WMS_Defects",
                        principalColumn: "DefectID");
                    table.ForeignKey(
                        name: "FK__WMS_Defec__Varia__589C25F3",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_TransferLines",
                columns: table => new
                {
                    TransLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Tran__67BAE7AD9E258FDB", x => x.TransLineID);
                    table.ForeignKey(
                        name: "FK__WMS_Trans__Trans__3DE82FB7",
                        column: x => x.TransferID,
                        principalTable: "WMS_Transfers",
                        principalColumn: "TransferID");
                    table.ForeignKey(
                        name: "FK__WMS_Trans__Varia__3EDC53F0",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_AdjustmentLines",
                columns: table => new
                {
                    AdjLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    AdjQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Adju__4B9DA6758B19690F", x => x.AdjLineID);
                    table.ForeignKey(
                        name: "FK__WMS_Adjus__AdjID__4F12BBB9",
                        column: x => x.AdjID,
                        principalTable: "WMS_Adjustments",
                        principalColumn: "AdjID");
                    table.ForeignKey(
                        name: "FK__WMS_Adjus__Varia__5006DFF2",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "WMS_StockBalances",
                columns: table => new
                {
                    StockID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseID = table.Column<int>(type: "int", nullable: true),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WMS_Stoc__2C83A9E21344FFEF", x => x.StockID);
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Locat__5D60DB10",
                        column: x => x.LocationID,
                        principalTable: "WMS_Locations",
                        principalColumn: "LocationID");
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Varia__5E54FF49",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                    table.ForeignKey(
                        name: "FK__WMS_Stock__Wareh__5C6CB6D7",
                        column: x => x.WarehouseID,
                        principalTable: "WMS_Warehouses",
                        principalColumn: "WarehouseID");
                });

            migrationBuilder.CreateTable(
                name: "LOG_ManifestLines",
                columns: table => new
                {
                    ManifestLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManifestID = table.Column<int>(type: "int", nullable: true),
                    DOID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOG_Mani__19271EBEE1D003B2", x => x.ManifestLineID);
                    table.ForeignKey(
                        name: "FK__LOG_Manif__Manif__2B947552",
                        column: x => x.ManifestID,
                        principalTable: "LOG_Manifests",
                        principalColumn: "ManifestID");
                    table.ForeignKey(
                        name: "FK__LOG_Manife__DOID__2C88998B",
                        column: x => x.DOID,
                        principalTable: "SAL_Deliveries",
                        principalColumn: "DOID");
                });

            migrationBuilder.CreateTable(
                name: "PUR_ReturnLines",
                columns: table => new
                {
                    PReturnLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PReturnID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    ReturnQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PUR_Retu__5213AFA28BA0D95E", x => x.PReturnLineID);
                    table.ForeignKey(
                        name: "FK__PUR_Retur__PRetu__01D345B0",
                        column: x => x.PReturnID,
                        principalTable: "PUR_Returns",
                        principalColumn: "PReturnID");
                    table.ForeignKey(
                        name: "FK__PUR_Retur__Varia__02C769E9",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateTable(
                name: "SAL_ReturnLines",
                columns: table => new
                {
                    SReturnLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SReturnID = table.Column<int>(type: "int", nullable: true),
                    VariantID = table.Column<int>(type: "int", nullable: true),
                    ReturnQty = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SAL_Retu__041A7130DDCB7575", x => x.SReturnLineID);
                    table.ForeignKey(
                        name: "FK__SAL_Retur__SRetu__2CBDA3B5",
                        column: x => x.SReturnID,
                        principalTable: "SAL_Returns",
                        principalColumn: "SReturnID");
                    table.ForeignKey(
                        name: "FK__SAL_Retur__Varia__2DB1C7EE",
                        column: x => x.VariantID,
                        principalTable: "ITM_Variants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRM_Addresses_DistrictID",
                table: "CRM_Addresses",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_Addresses_PartnerID",
                table: "CRM_Addresses",
                column: "PartnerID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_BankAccounts_PartnerID",
                table: "CRM_BankAccounts",
                column: "PartnerID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_Contacts_PartnerID",
                table: "CRM_Contacts",
                column: "PartnerID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_Contracts_PartnerID",
                table: "CRM_Contracts",
                column: "PartnerID");

            migrationBuilder.CreateIndex(
                name: "UQ__CRM_Cont__C908F4B8EEAEE777",
                table: "CRM_Contracts",
                column: "ContractNo",
                unique: true,
                filter: "[ContractNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_CustomerClasses_CreatedBy",
                table: "CRM_CustomerClasses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_PartnerGroups_CreatedBy",
                table: "CRM_PartnerGroups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__CRM_Part__3B9743800E94A435",
                table: "CRM_PartnerGroups",
                column: "GroupCode",
                unique: true,
                filter: "[GroupCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_Partners_GroupID",
                table: "CRM_Partners",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "UQ__CRM_Part__E6792F57A24E7A17",
                table: "CRM_Partners",
                column: "PartnerCode",
                unique: true,
                filter: "[PartnerCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_PriceLists_CreatedBy",
                table: "CRM_PriceLists",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__CRM_Pric__287292B8AA303233",
                table: "CRM_PriceLists",
                column: "ListCode",
                unique: true,
                filter: "[ListCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_SupplierEvals_EvaluatorID",
                table: "CRM_SupplierEvals",
                column: "EvaluatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CRM_SupplierEvals_SupplierID",
                table: "CRM_SupplierEvals",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AP_InvoiceLines_APInvID",
                table: "FIN_AP_InvoiceLines",
                column: "APInvID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AP_Invoices_GRNID",
                table: "FIN_AP_Invoices",
                column: "GRNID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AP_Invoices_SupplierID",
                table: "FIN_AP_Invoices",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AR_InvoiceLines_ARInvID",
                table: "FIN_AR_InvoiceLines",
                column: "ARInvID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AR_Invoices_CustomerID",
                table: "FIN_AR_Invoices",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_AR_Invoices_DOID",
                table: "FIN_AR_Invoices",
                column: "DOID");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Cash__38D0C56A6405293D",
                table: "FIN_CashAccounts",
                column: "AccountCode",
                unique: true,
                filter: "[AccountCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Char__38D0C56A577D30C8",
                table: "FIN_ChartOfAccounts",
                column: "AccountCode",
                unique: true,
                filter: "[AccountCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_CostCenters_CreatedBy",
                table: "FIN_CostCenters",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Cost__55D5E3C6237E870D",
                table: "FIN_CostCenters",
                column: "CenterCode",
                unique: true,
                filter: "[CenterCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Depreciations_AssetID",
                table: "FIN_Depreciations",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_FiscalPeriods_ClosedBy",
                table: "FIN_FiscalPeriods",
                column: "ClosedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_FixedAssets_RecordedBy",
                table: "FIN_FixedAssets",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Fixe__2DDE5240F6FF20CA",
                table: "FIN_FixedAssets",
                column: "AssetCode",
                unique: true,
                filter: "[AssetCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_JournalLines_AccountID",
                table: "FIN_JournalLines",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_JournalLines_JournalID",
                table: "FIN_JournalLines",
                column: "JournalID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Journals_CreatorID",
                table: "FIN_Journals",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Jour__250148D5BF949E58",
                table: "FIN_Journals",
                column: "JournalNo",
                unique: true,
                filter: "[JournalNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Payments_BankAccID",
                table: "FIN_Payments",
                column: "BankAccID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Payments_CashID",
                table: "FIN_Payments",
                column: "CashID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Payments_CreatorID",
                table: "FIN_Payments",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Payments_SupplierID",
                table: "FIN_Payments",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_Paym__675CC10D88F7B665",
                table: "FIN_PaymentTerms",
                column: "TermCode",
                unique: true,
                filter: "[TermCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Receipts_BankAccID",
                table: "FIN_Receipts",
                column: "BankAccID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Receipts_CashID",
                table: "FIN_Receipts",
                column: "CashID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Receipts_CreatorID",
                table: "FIN_Receipts",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "IX_FIN_Receipts_CustomerID",
                table: "FIN_Receipts",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "UQ__FIN_TaxR__12945A28CDE4F663",
                table: "FIN_TaxRates",
                column: "TaxCode",
                unique: true,
                filter: "[TaxCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__GEO_Coun__5D9B0D2C57D8C954",
                table: "GEO_Countries",
                column: "CountryCode",
                unique: true,
                filter: "[CountryCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GEO_Districts_ProvinceID",
                table: "GEO_Districts",
                column: "ProvinceID");

            migrationBuilder.CreateIndex(
                name: "IX_GEO_Provinces_CountryID",
                table: "GEO_Provinces",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Branches_DistrictID",
                table: "HRM_Branches",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Branches_ManagerId",
                table: "HRM_Branches",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "UQ__HRM_Bran__1C61B8880DCD4BFC",
                table: "HRM_Branches",
                column: "BranchCode",
                unique: true,
                filter: "[BranchCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Departments_BranchID",
                table: "HRM_Departments",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "UQ__HRM_Depa__BB9B9550C5C793F9",
                table: "HRM_Departments",
                column: "DeptCode",
                unique: true,
                filter: "[DeptCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Employees_BranchId",
                table: "HRM_Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Employees_DepartmentID",
                table: "HRM_Employees",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Employees_TitleID",
                table: "HRM_Employees",
                column: "TitleID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Employees_WarehouseId",
                table: "HRM_Employees",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "UQ__HRM_Empl__7DA847CA6FA867AC",
                table: "HRM_Employees",
                column: "EmpCode",
                unique: true,
                filter: "[EmpCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__HRM_JobT__8B388717B742A043",
                table: "HRM_JobTitles",
                column: "TitleCode",
                unique: true,
                filter: "[TitleCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Payroll_EmployeeID",
                table: "HRM_Payroll",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Timesheets_EmployeeID",
                table: "HRM_Timesheets",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_HRM_Timesheets_ShiftID",
                table: "HRM_Timesheets",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Attributes_CreatedBy",
                table: "ITM_Attributes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Barcodes_VariantID",
                table: "ITM_Barcodes",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Barc__177800D347B8B07A",
                table: "ITM_Barcodes",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Batches_SupplierID",
                table: "ITM_Batches",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Batches_VariantID",
                table: "ITM_Batches",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Bran__44292CC70A7E54BB",
                table: "ITM_Brands",
                column: "BrandCode",
                unique: true,
                filter: "[BrandCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Cate__5E593E4EE9ADD03C",
                table: "ITM_Categories",
                column: "CatCode",
                unique: true,
                filter: "[CatCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Images_ProductID",
                table: "ITM_Images",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_PriceListDetails_VariantID",
                table: "ITM_PriceListDetails",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Products_BrandID",
                table: "ITM_Products",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Products_SubCatID",
                table: "ITM_Products",
                column: "SubCatID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Products_TaxID",
                table: "ITM_Products",
                column: "TaxID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Products_UoMGroupID",
                table: "ITM_Products",
                column: "UoMGroupID");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Prod__CA1ECF0DD1C8A883",
                table: "ITM_Products",
                column: "SKU",
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Serials_VariantID",
                table: "ITM_Serials",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Seri__5E5A535E27403A6B",
                table: "ITM_Serials",
                column: "SerialNo",
                unique: true,
                filter: "[SerialNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_SubCategories_CategoryID",
                table: "ITM_SubCategories",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_UoMConversions_UoMGroupID",
                table: "ITM_UoMConversions",
                column: "UoMGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_VariantAttributes_AttrID",
                table: "ITM_VariantAttributes",
                column: "AttrID");

            migrationBuilder.CreateIndex(
                name: "IX_ITM_Variants_ProductID",
                table: "ITM_Variants",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "UQ__ITM_Vari__E55CDF9721810930",
                table: "ITM_Variants",
                column: "VariantSKU",
                unique: true,
                filter: "[VariantSKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Drivers_EmployeeID",
                table: "LOG_Drivers",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_ManifestLines_DOID",
                table: "LOG_ManifestLines",
                column: "DOID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_ManifestLines_ManifestID",
                table: "LOG_ManifestLines",
                column: "ManifestID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Manifests_DispatcherID",
                table: "LOG_Manifests",
                column: "DispatcherID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Manifests_DriverID",
                table: "LOG_Manifests",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Manifests_RouteID",
                table: "LOG_Manifests",
                column: "RouteID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Manifests_VehicleID",
                table: "LOG_Manifests",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Routes_CreatedBy",
                table: "LOG_Routes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__LOG_Rout__FDC34585901686C9",
                table: "LOG_Routes",
                column: "RouteCode",
                unique: true,
                filter: "[RouteCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_Vehicles_ManagedBy",
                table: "LOG_Vehicles",
                column: "ManagedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__LOG_Vehi__026BC15CC12A33BD",
                table: "LOG_Vehicles",
                column: "LicensePlate",
                unique: true,
                filter: "[LicensePlate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_OrderLines_POID",
                table: "PUR_OrderLines",
                column: "POID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_OrderLines_VariantID",
                table: "PUR_OrderLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Orders_SupplierID",
                table: "PUR_Orders",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Orders_TermID",
                table: "PUR_Orders",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "UQ__PUR_Orde__40ACF5B896A39BD2",
                table: "PUR_Orders",
                column: "POCode",
                unique: true,
                filter: "[POCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_ReceiptLines_GRNID",
                table: "PUR_ReceiptLines",
                column: "GRNID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_ReceiptLines_POLineID",
                table: "PUR_ReceiptLines",
                column: "POLineID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Receipts_POID",
                table: "PUR_Receipts",
                column: "POID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Receipts_ReceiverID",
                table: "PUR_Receipts",
                column: "ReceiverID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Receipts_WarehouseID",
                table: "PUR_Receipts",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "UQ__PUR_Rece__F1E8DDCB620A3163",
                table: "PUR_Receipts",
                column: "GRNCode",
                unique: true,
                filter: "[GRNCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_RequestLines_PRID",
                table: "PUR_RequestLines",
                column: "PRID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_RequestLines_VariantID",
                table: "PUR_RequestLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Requests_RequesterID",
                table: "PUR_Requests",
                column: "RequesterID");

            migrationBuilder.CreateIndex(
                name: "UQ__PUR_Requ__9E4C02EBFB5B2389",
                table: "PUR_Requests",
                column: "PRCode",
                unique: true,
                filter: "[PRCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_ReturnLines_PReturnID",
                table: "PUR_ReturnLines",
                column: "PReturnID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_ReturnLines_VariantID",
                table: "PUR_ReturnLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Returns_CreatorID",
                table: "PUR_Returns",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_Returns_GRNID",
                table: "PUR_Returns",
                column: "GRNID");

            migrationBuilder.CreateIndex(
                name: "UQ__PUR_Retu__4CF726C9C68FC9E9",
                table: "PUR_Returns",
                column: "ReturnCode",
                unique: true,
                filter: "[ReturnCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_SupplierQuoteLines_QuoteID",
                table: "PUR_SupplierQuoteLines",
                column: "QuoteID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_SupplierQuoteLines_VariantID",
                table: "PUR_SupplierQuoteLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_SupplierQuotes_CreatedBy",
                table: "PUR_SupplierQuotes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PUR_SupplierQuotes_SupplierID",
                table: "PUR_SupplierQuotes",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Deliveries_DispatcherID",
                table: "SAL_Deliveries",
                column: "DispatcherID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Deliveries_SOID",
                table: "SAL_Deliveries",
                column: "SOID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Deliveries_WarehouseID",
                table: "SAL_Deliveries",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "UQ__SAL_Deli__33C65279AD860C60",
                table: "SAL_Deliveries",
                column: "DOCode",
                unique: true,
                filter: "[DOCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_DeliveryLines_DOID",
                table: "SAL_DeliveryLines",
                column: "DOID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_DeliveryLines_SOLineID",
                table: "SAL_DeliveryLines",
                column: "SOLineID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_OrderLines_SOID",
                table: "SAL_OrderLines",
                column: "SOID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_OrderLines_VariantID",
                table: "SAL_OrderLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_OrderPromotions_PromoID",
                table: "SAL_OrderPromotions",
                column: "PromoID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Orders_CustomerID",
                table: "SAL_Orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Orders_TermID",
                table: "SAL_Orders",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "UQ__SAL_Orde__27B177430A95713C",
                table: "SAL_Orders",
                column: "SOCode",
                unique: true,
                filter: "[SOCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Promotions_CreatedBy",
                table: "SAL_Promotions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__SAL_Prom__32DBED354F211F0C",
                table: "SAL_Promotions",
                column: "PromoCode",
                unique: true,
                filter: "[PromoCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_QuotationLines_SQID",
                table: "SAL_QuotationLines",
                column: "SQID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_QuotationLines_VariantID",
                table: "SAL_QuotationLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Quotations_CreatorID",
                table: "SAL_Quotations",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Quotations_CustomerID",
                table: "SAL_Quotations",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "UQ__SAL_Quot__0CBC62E05016F503",
                table: "SAL_Quotations",
                column: "SQCode",
                unique: true,
                filter: "[SQCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_ReturnLines_SReturnID",
                table: "SAL_ReturnLines",
                column: "SReturnID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_ReturnLines_VariantID",
                table: "SAL_ReturnLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Returns_DOID",
                table: "SAL_Returns",
                column: "DOID");

            migrationBuilder.CreateIndex(
                name: "IX_SAL_Returns_ReceiverID",
                table: "SAL_Returns",
                column: "ReceiverID");

            migrationBuilder.CreateIndex(
                name: "UQ__SAL_Retu__4CF726C91C8F8E5B",
                table: "SAL_Returns",
                column: "ReturnCode",
                unique: true,
                filter: "[ReturnCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_AuditLogs_UserID",
                table: "SYS_AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_EmailTemplates_CreatedBy",
                table: "SYS_EmailTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_Emai__A25C5AA79BCF86B7",
                table: "SYS_EmailTemplates",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_ErrorLogs_UserID",
                table: "SYS_ErrorLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_Features_ModuleID",
                table: "SYS_Features",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_Feat__75CE31548B7A0957",
                table: "SYS_Features",
                column: "FeatureCode",
                unique: true,
                filter: "[FeatureCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_Modu__EB27D4332979CFC6",
                table: "SYS_Modules",
                column: "ModuleCode",
                unique: true,
                filter: "[ModuleCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_RoleFeatures_FeatureID",
                table: "SYS_RoleFeatures",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_Role__D62CB59C4B1C64E1",
                table: "SYS_Roles",
                column: "RoleCode",
                unique: true,
                filter: "[RoleCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_Settings_UpdatedBy",
                table: "SYS_Settings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_Sett__01E719AD967E8DC7",
                table: "SYS_Settings",
                column: "SettingKey",
                unique: true,
                filter: "[SettingKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_UiLogs_UserId",
                table: "SYS_UiLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SYS_UserRoles_RoleID",
                table: "SYS_UserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_User__536C85E4D854B446",
                table: "SYS_Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__SYS_User__7AD04FF06971FC95",
                table: "SYS_Users",
                column: "EmployeeID",
                unique: true,
                filter: "[EmployeeID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_AdjustmentLines_AdjID",
                table: "WMS_AdjustmentLines",
                column: "AdjID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_AdjustmentLines_VariantID",
                table: "WMS_AdjustmentLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Adjustments_ApproverID",
                table: "WMS_Adjustments",
                column: "ApproverID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Adjustments_CheckID",
                table: "WMS_Adjustments",
                column: "CheckID");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_Adju__FA7B48382F1ED6D5",
                table: "WMS_Adjustments",
                column: "AdjCode",
                unique: true,
                filter: "[AdjCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_DefectLines_DefectID",
                table: "WMS_DefectLines",
                column: "DefectID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_DefectLines_VariantID",
                table: "WMS_DefectLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Defects_ReporterID",
                table: "WMS_Defects",
                column: "ReporterID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Defects_WarehouseID",
                table: "WMS_Defects",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_Defe__EB5D7BE5ADD22BDA",
                table: "WMS_Defects",
                column: "DefectCode",
                unique: true,
                filter: "[DefectCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_InvCheckLines_CheckID",
                table: "WMS_InvCheckLines",
                column: "CheckID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_InvCheckLines_VariantID",
                table: "WMS_InvCheckLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_InvChecks_CheckerID",
                table: "WMS_InvChecks",
                column: "CheckerID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_InvChecks_WarehouseID",
                table: "WMS_InvChecks",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_InvC__3DD1954B92E2CB8E",
                table: "WMS_InvChecks",
                column: "CheckCode",
                unique: true,
                filter: "[CheckCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Locations_RackID",
                table: "WMS_Locations",
                column: "RackID");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_Loca__DDB144D5E877EC8A",
                table: "WMS_Locations",
                column: "LocationCode",
                unique: true,
                filter: "[LocationCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_LocationTypes_CreatedBy",
                table: "WMS_LocationTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Racks_ZoneID",
                table: "WMS_Racks",
                column: "ZoneID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_StockBalances_LocationID",
                table: "WMS_StockBalances",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_StockBalances_VariantID",
                table: "WMS_StockBalances",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "UQ_Stock",
                table: "WMS_StockBalances",
                columns: new[] { "WarehouseID", "LocationID", "VariantID" },
                unique: true,
                filter: "[WarehouseID] IS NOT NULL AND [LocationID] IS NOT NULL AND [VariantID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_StockLedger_CreatedBy",
                table: "WMS_StockLedger",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_StockLedger_VariantID",
                table: "WMS_StockLedger",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_StockLedger_WarehouseID",
                table: "WMS_StockLedger",
                column: "WarehouseID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_TransferLines_TransferID",
                table: "WMS_TransferLines",
                column: "TransferID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_TransferLines_VariantID",
                table: "WMS_TransferLines",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Transfers_CreatorID",
                table: "WMS_Transfers",
                column: "CreatorID");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Transfers_FromWH",
                table: "WMS_Transfers",
                column: "FromWH");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Transfers_ToWH",
                table: "WMS_Transfers",
                column: "ToWH");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_Tran__CE99A4C5FA478317",
                table: "WMS_Transfers",
                column: "TransferCode",
                unique: true,
                filter: "[TransferCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Warehouses_BranchID",
                table: "WMS_Warehouses",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "UQ__WMS_Ware__3EB0FEFDEFE40730",
                table: "WMS_Warehouses",
                column: "WHCode",
                unique: true,
                filter: "[WHCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WMS_Zones_WarehouseID",
                table: "WMS_Zones",
                column: "WarehouseID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Addre__Partn__07C12930",
                table: "CRM_Addresses",
                column: "PartnerID",
                principalTable: "CRM_Partners",
                principalColumn: "PartnerID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_BankA__Partn__0E6E26BF",
                table: "CRM_BankAccounts",
                column: "PartnerID",
                principalTable: "CRM_Partners",
                principalColumn: "PartnerID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Conta__Partn__0B91BA14",
                table: "CRM_Contacts",
                column: "PartnerID",
                principalTable: "CRM_Partners",
                principalColumn: "PartnerID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Contr__Partn__18EBB532",
                table: "CRM_Contracts",
                column: "PartnerID",
                principalTable: "CRM_Partners",
                principalColumn: "PartnerID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Custo__Creat__151B244E",
                table: "CRM_CustomerClasses",
                column: "CreatedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Partn__Creat__01142BA1",
                table: "CRM_PartnerGroups",
                column: "CreatedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Price__Creat__1CBC4616",
                table: "CRM_PriceLists",
                column: "CreatedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__CRM_Suppl__Evalu__123EB7A3",
                table: "CRM_SupplierEvals",
                column: "EvaluatorID",
                principalTable: "HRM_Employees",
                principalColumn: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_AP_In__APInv__7FB5F314",
                table: "FIN_AP_InvoiceLines",
                column: "APInvID",
                principalTable: "FIN_AP_Invoices",
                principalColumn: "APInvID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_AP_In__GRNID__7CD98669",
                table: "FIN_AP_Invoices",
                column: "GRNID",
                principalTable: "PUR_Receipts",
                principalColumn: "GRNID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_AR_In__ARInv__0662F0A3",
                table: "FIN_AR_InvoiceLines",
                column: "ARInvID",
                principalTable: "FIN_AR_Invoices",
                principalColumn: "ARInvID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_AR_Inv__DOID__038683F8",
                table: "FIN_AR_Invoices",
                column: "DOID",
                principalTable: "SAL_Deliveries",
                principalColumn: "DOID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_CostC__Creat__69C6B1F5",
                table: "FIN_CostCenters",
                column: "CreatedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Depre__Asset__1881A0DE",
                table: "FIN_Depreciations",
                column: "AssetID",
                principalTable: "FIN_FixedAssets",
                principalColumn: "AssetID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Fisca__Close__6CA31EA0",
                table: "FIN_FiscalPeriods",
                column: "ClosedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Fixed__Recor__15A53433",
                table: "FIN_FixedAssets",
                column: "RecordedBy",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Journ__Journ__73501C2F",
                table: "FIN_JournalLines",
                column: "JournalID",
                principalTable: "FIN_Journals",
                principalColumn: "JournalID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Journ__Creat__7073AF84",
                table: "FIN_Journals",
                column: "CreatorID",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Payme__Creat__0C1BC9F9",
                table: "FIN_Payments",
                column: "CreatorID",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__FIN_Recei__Creat__11D4A34F",
                table: "FIN_Receipts",
                column: "CreatorID",
                principalTable: "SYS_Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_HRM_Branches_HRM_Employees_ManagerId",
                table: "HRM_Branches",
                column: "ManagerId",
                principalTable: "HRM_Employees",
                principalColumn: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__HRM_Branc__Distr__412EB0B6",
                table: "HRM_Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_HRM_Branches_HRM_Employees_ManagerId",
                table: "HRM_Branches");

            migrationBuilder.DropTable(
                name: "CRM_Addresses");

            migrationBuilder.DropTable(
                name: "CRM_BankAccounts");

            migrationBuilder.DropTable(
                name: "CRM_Contacts");

            migrationBuilder.DropTable(
                name: "CRM_Contracts");

            migrationBuilder.DropTable(
                name: "CRM_CustomerClasses");

            migrationBuilder.DropTable(
                name: "CRM_SupplierEvals");

            migrationBuilder.DropTable(
                name: "FIN_AP_InvoiceLines");

            migrationBuilder.DropTable(
                name: "FIN_AR_InvoiceLines");

            migrationBuilder.DropTable(
                name: "FIN_CostCenters");

            migrationBuilder.DropTable(
                name: "FIN_Depreciations");

            migrationBuilder.DropTable(
                name: "FIN_FiscalPeriods");

            migrationBuilder.DropTable(
                name: "FIN_JournalLines");

            migrationBuilder.DropTable(
                name: "FIN_Payments");

            migrationBuilder.DropTable(
                name: "FIN_Receipts");

            migrationBuilder.DropTable(
                name: "HRM_Payroll");

            migrationBuilder.DropTable(
                name: "HRM_Timesheets");

            migrationBuilder.DropTable(
                name: "ITM_Barcodes");

            migrationBuilder.DropTable(
                name: "ITM_Batches");

            migrationBuilder.DropTable(
                name: "ITM_Images");

            migrationBuilder.DropTable(
                name: "ITM_PriceListDetails");

            migrationBuilder.DropTable(
                name: "ITM_Serials");

            migrationBuilder.DropTable(
                name: "ITM_UoMConversions");

            migrationBuilder.DropTable(
                name: "ITM_VariantAttributes");

            migrationBuilder.DropTable(
                name: "LOG_ManifestLines");

            migrationBuilder.DropTable(
                name: "PUR_ReceiptLines");

            migrationBuilder.DropTable(
                name: "PUR_RequestLines");

            migrationBuilder.DropTable(
                name: "PUR_ReturnLines");

            migrationBuilder.DropTable(
                name: "PUR_SupplierQuoteLines");

            migrationBuilder.DropTable(
                name: "SAL_DeliveryLines");

            migrationBuilder.DropTable(
                name: "SAL_OrderPromotions");

            migrationBuilder.DropTable(
                name: "SAL_QuotationLines");

            migrationBuilder.DropTable(
                name: "SAL_ReturnLines");

            migrationBuilder.DropTable(
                name: "SYS_AuditLogs");

            migrationBuilder.DropTable(
                name: "SYS_EmailTemplates");

            migrationBuilder.DropTable(
                name: "SYS_ErrorLogs");

            migrationBuilder.DropTable(
                name: "SYS_RoleFeatures");

            migrationBuilder.DropTable(
                name: "SYS_Settings");

            migrationBuilder.DropTable(
                name: "SYS_UiLogs");

            migrationBuilder.DropTable(
                name: "SYS_UserRoles");

            migrationBuilder.DropTable(
                name: "WMS_AdjustmentLines");

            migrationBuilder.DropTable(
                name: "WMS_DefectLines");

            migrationBuilder.DropTable(
                name: "WMS_InvCheckLines");

            migrationBuilder.DropTable(
                name: "WMS_LocationTypes");

            migrationBuilder.DropTable(
                name: "WMS_StockBalances");

            migrationBuilder.DropTable(
                name: "WMS_StockLedger");

            migrationBuilder.DropTable(
                name: "WMS_TransferLines");

            migrationBuilder.DropTable(
                name: "FIN_AP_Invoices");

            migrationBuilder.DropTable(
                name: "FIN_AR_Invoices");

            migrationBuilder.DropTable(
                name: "FIN_FixedAssets");

            migrationBuilder.DropTable(
                name: "FIN_ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "FIN_Journals");

            migrationBuilder.DropTable(
                name: "FIN_BankAccounts");

            migrationBuilder.DropTable(
                name: "FIN_CashAccounts");

            migrationBuilder.DropTable(
                name: "HRM_Shifts");

            migrationBuilder.DropTable(
                name: "CRM_PriceLists");

            migrationBuilder.DropTable(
                name: "ITM_Attributes");

            migrationBuilder.DropTable(
                name: "LOG_Manifests");

            migrationBuilder.DropTable(
                name: "PUR_OrderLines");

            migrationBuilder.DropTable(
                name: "PUR_Requests");

            migrationBuilder.DropTable(
                name: "PUR_Returns");

            migrationBuilder.DropTable(
                name: "PUR_SupplierQuotes");

            migrationBuilder.DropTable(
                name: "SAL_OrderLines");

            migrationBuilder.DropTable(
                name: "SAL_Promotions");

            migrationBuilder.DropTable(
                name: "SAL_Quotations");

            migrationBuilder.DropTable(
                name: "SAL_Returns");

            migrationBuilder.DropTable(
                name: "SYS_Features");

            migrationBuilder.DropTable(
                name: "SYS_Roles");

            migrationBuilder.DropTable(
                name: "WMS_Adjustments");

            migrationBuilder.DropTable(
                name: "WMS_Defects");

            migrationBuilder.DropTable(
                name: "WMS_Locations");

            migrationBuilder.DropTable(
                name: "WMS_Transfers");

            migrationBuilder.DropTable(
                name: "LOG_Drivers");

            migrationBuilder.DropTable(
                name: "LOG_Routes");

            migrationBuilder.DropTable(
                name: "LOG_Vehicles");

            migrationBuilder.DropTable(
                name: "PUR_Receipts");

            migrationBuilder.DropTable(
                name: "ITM_Variants");

            migrationBuilder.DropTable(
                name: "SAL_Deliveries");

            migrationBuilder.DropTable(
                name: "SYS_Modules");

            migrationBuilder.DropTable(
                name: "WMS_InvChecks");

            migrationBuilder.DropTable(
                name: "WMS_Racks");

            migrationBuilder.DropTable(
                name: "PUR_Orders");

            migrationBuilder.DropTable(
                name: "ITM_Products");

            migrationBuilder.DropTable(
                name: "SAL_Orders");

            migrationBuilder.DropTable(
                name: "WMS_Zones");

            migrationBuilder.DropTable(
                name: "ITM_Brands");

            migrationBuilder.DropTable(
                name: "ITM_SubCategories");

            migrationBuilder.DropTable(
                name: "FIN_TaxRates");

            migrationBuilder.DropTable(
                name: "ITM_UoMGroups");

            migrationBuilder.DropTable(
                name: "CRM_Partners");

            migrationBuilder.DropTable(
                name: "FIN_PaymentTerms");

            migrationBuilder.DropTable(
                name: "ITM_Categories");

            migrationBuilder.DropTable(
                name: "CRM_PartnerGroups");

            migrationBuilder.DropTable(
                name: "SYS_Users");

            migrationBuilder.DropTable(
                name: "GEO_Districts");

            migrationBuilder.DropTable(
                name: "GEO_Provinces");

            migrationBuilder.DropTable(
                name: "GEO_Countries");

            migrationBuilder.DropTable(
                name: "HRM_Employees");

            migrationBuilder.DropTable(
                name: "WMS_Warehouses");

            migrationBuilder.DropTable(
                name: "HRM_Departments");

            migrationBuilder.DropTable(
                name: "HRM_JobTitles");

            migrationBuilder.DropTable(
                name: "HRM_Branches");
        }
    }
}
