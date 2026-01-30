using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.API.Models.Entities
{
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }
        public int MaND { get; set; }
        public int MaSP { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;

        // Navigation
        public virtual NguoiDung? NguoiDung { get; set; }
        public virtual SanPham? SanPham { get; set; }
    }
}
