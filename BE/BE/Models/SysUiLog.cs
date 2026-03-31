using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Models // <-- Đã sửa cho khớp với dự án của bạn
{
    [Table("SysUiLog")]
    public class SysUiLog
    {
        [Key]
        public int Id { get; set; }

        // --- KHÓA NGOẠI LIÊN KẾT VỚI BẢNG SysUser ---
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual SysUser SysUser { get; set; }

        // Lưu thêm tên đăng nhập để query cho nhanh không cần Join
        [StringLength(100)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string EventType { get; set; } // NAVIGATION, CLICK, API_CALL, ERROR

        [StringLength(255)]
        public string Path { get; set; } // Đường dẫn URL lúc xảy ra sự kiện

        [StringLength(500)]
        public string Message { get; set; } // Mô tả ngắn gọn sự kiện

        public string Details { get; set; } // Chuỗi JSON chứa dữ liệu chi tiết

        public DateTime LogDate { get; set; } = DateTime.Now; // Thời gian ghi log
    }
}