using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <-- THÊM DÒNG NÀY

namespace BE.Models
{
    [Table("Sys_UiLogs")]
    public class SysUiLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        // === SỬA DÒNG NÀY: Thêm JsonIgnore và dấu ? ===
        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual SysUser? SysUser { get; set; }
        // ===============================================

        [StringLength(100)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string EventType { get; set; }

        [StringLength(255)]
        public string Path { get; set; }

        [StringLength(500)]
        public string Message { get; set; }

        public string Details { get; set; }

        public DateTime LogDate { get; set; } = DateTime.Now;
    }
} 