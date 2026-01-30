using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.API.Models.Entities
{
    public class ThongKeLuotXem
    {
        [Key]
        public int MaTK { get; set; }
        public int MaSP { get; set; }
        public int? MaND { get; set; }

        [Required]
        public DateTime NgayXem { get; set; }

        public int SoLuotXemTrongNgay { get; set; } = 1;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;

        // Navigation
        public virtual SanPham? SanPham { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
