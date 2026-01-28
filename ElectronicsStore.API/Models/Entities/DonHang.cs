using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    public class DonHang
    {
        [Key]
        public int MaDH { get; set; }

        [Required]
        [StringLength(50)]
        public string MaDonHangGoc { get; set; } = string.Empty;

        public int MaND { get; set; }
        public DateTime NgayTaoDon { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TongGiaTruocGiam { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PhiVanChuyen { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TienGiamGia { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TongGiaSauGiam { get; set; }

        [StringLength(50)]
        public string TrangThaiDon { get; set; } = "Chờ xác nhận";

        [StringLength(50)]
        public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán";

        [StringLength(100)]
        public string? PhuongThucThanhToan { get; set; }

        [StringLength(500)]
        public string? DiaChiGiaoHang { get; set; }

        [StringLength(100)]
        public string? ThanhPhoPhuong { get; set; }

        [StringLength(20)]
        public string? SodTLienHe { get; set; }

        public string? GhiChu { get; set; }
        public DateTime? NgayCapNhat { get; set; }

        // Navigation
        public virtual NguoiDung? NguoiDung { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
        public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; } = new List<LichSuDonHang>();
    }
}
