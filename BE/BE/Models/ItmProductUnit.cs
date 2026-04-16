using BE.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tên_Project_Của_Sếp.Models
{
    [Table("ITM_ProductUnits")]
    public partial class ItmProductUnit
    {
        [Key]
        [Column("ConversionID")]
        public int ConversionId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        [Column("FromUnitID")]
        public int FromUnitId { get; set; }

        [Column("FromQty", TypeName = "decimal(18, 4)")]
        public decimal FromQty { get; set; }

        [Column("ToUnitID")]
        public int ToUnitId { get; set; }

        [Column("ToQty", TypeName = "decimal(18, 4)")]
        public decimal ToQty { get; set; }

        // Khai báo 3 sợi dây Khóa Ngoại
        [ForeignKey("ProductId")]
        public virtual ItmProduct Product { get; set; } = null!;

        [ForeignKey("FromUnitId")]
        public virtual ItmUnit FromUnit { get; set; } = null!;

        [ForeignKey("ToUnitId")]
        public virtual ItmUnit ToUnit { get; set; } = null!;
    }
}