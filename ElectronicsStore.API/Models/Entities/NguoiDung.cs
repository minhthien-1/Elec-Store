using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.API.Models.Entities
{
    public class NguoiDung
    {
        [Key]
        public int MaND { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string TenDayDu { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhauHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        [StringLength(500)]
        public string? DiaChiChiTiet { get; set; }

        [StringLength(100)]
        public string? ThanhPho { get; set; }

        [StringLength(100)]
        public string QuocGia { get; set; } = "Việt Nam";

        [StringLength(500)]
        public string? DiaChiMacDinh { get; set; }

        public bool LaQuanTriVien { get; set; } = false;
        public bool DangHoatDong { get; set; } = true;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; }

        // Navigation
        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
        public virtual ICollection<DanhGiaSanPham> DanhGias { get; set; } = new List<DanhGiaSanPham>();
        public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
        public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; } = new List<LichSuDonHang>();
    }
}
