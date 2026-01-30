using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    public class LichSuThanhToan
    {
        [Key]
        public int MaLS { get; set; }
        public int MaDH { get; set; }
        public int MaND { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal SoTienThanhToan { get; set; }

        [StringLength(100)]
        public string? PhuongThuc { get; set; }

        [StringLength(50)]
        public string? TrangThaiGD { get; set; } // 'Thành công', 'Thất bại', 'Đang xử lý'

        [StringLength(100)]
        public string? MaThamChieu { get; set; }

        public string? GhiChuLoi { get; set; }
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        // Navigation
        public virtual DonHang? DonHang { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
