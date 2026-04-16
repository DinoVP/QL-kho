using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Models
{
    [Table("ITM_PriceHistories")]
    public class ItmPriceHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int VariantId { get; set; }

        [ForeignKey("VariantId")]
        public virtual ItmVariant? Variant { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewPrice { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        [MaxLength(255)]
        public string? Source { get; set; }
    }
}