using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }
        public int MaDH { get; set; }
        public int MaSP { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal GiaTaiThoiDiem { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal GiamGiaCungChiTiet { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal ThanhTien { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;

        // Navigation
        public virtual DonHang? DonHang { get; set; }
        public virtual SanPham? SanPham { get; set; }
    }
}
