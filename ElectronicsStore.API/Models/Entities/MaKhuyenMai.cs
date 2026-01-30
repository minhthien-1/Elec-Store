using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    public class MaKhuyenMai
    {
        [Key]
        public int MaKM { get; set; }

        [Required]
        [StringLength(50)]
        public string MaCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TenChienDich { get; set; }

        public string? MoTa { get; set; }

        [StringLength(20)]
        public string? KieuGiam { get; set; } // 'PhanTram' hoặc 'TienCoThuc'

        [Column(TypeName = "decimal(10,2)")]
        public decimal? GiaTriGiam { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? GiaTriGiamToiDa { get; set; }

        public int SoLuotSuDung { get; set; } = 0;
        public int? GioiHanSoLuotSuDung { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? GiaTriDonHangToiThieu { get; set; }

        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
    }
}
