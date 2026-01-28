using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.API.Models.Entities
{
    public class DanhGiaSanPham
    {
        [Key]
        public int MaDG { get; set; }
        public int MaSP { get; set; }
        public int MaND { get; set; }

        [Required]
        [Range(1, 5)]
        public int DemSao { get; set; }

        [StringLength(200)]
        public string? TieuDe { get; set; }

        public string? NoiDung { get; set; }
        public bool CoDienTieuDung { get; set; } = false;
        public bool DuocDuyet { get; set; } = false;
        public int SoLuotThichChu { get; set; } = 0;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; }

        // Navigation
        public virtual SanPham? SanPham { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
