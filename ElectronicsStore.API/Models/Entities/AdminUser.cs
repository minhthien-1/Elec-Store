using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    [Table("NguoiDungs")]
    public class AdminUser
    {
        [Key]
        public int MaND { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string TenDayDu { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; } // Thêm trường này

        [Required]
        public string MatKhauHash { get; set; } = string.Empty;

        public string? DiaChiChiTiet { get; set; } // Khớp với ảnh sơ đồ DB
        public string? ThanhPho { get; set; }
        public string QuocGia { get; set; } = "Vietnam";

        public bool LaQuanTriVien { get; set; } = true;
        public bool DangHoatDong { get; set; } = true;

        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; } // Khớp với ảnh sơ đồ DB

        public string? Role { get; set; }
    }
}