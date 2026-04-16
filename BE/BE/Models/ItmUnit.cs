using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tên_Project_Của_Sếp.Models // Sếp nhớ đổi dòng này theo namespace thực tế nhé
{
    [Table("ITM_Units")]
    public partial class ItmUnit
    {
        [Key]
        [Column("UnitID")]
        public int UnitId { get; set; }

        [Column("UnitCode")]
        [StringLength(50)]
        public string UnitCode { get; set; } = null!;

        [Column("UnitName")]
        [StringLength(100)]
        public string UnitName { get; set; } = null!;

        // Khai báo móc nối (InverseProperty giúp EF Core hiểu sợi dây nào đi với list nào)
        [InverseProperty("FromUnit")]
        public virtual ICollection<ItmProductUnit> FromProductUnits { get; set; } = new List<ItmProductUnit>();

        [InverseProperty("ToUnit")]
        public virtual ICollection<ItmProductUnit> ToProductUnits { get; set; } = new List<ItmProductUnit>();
    }
}