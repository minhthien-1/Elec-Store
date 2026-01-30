using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    public class SanPham
    {
        [Key]
        public int MaSP { get; set; }

        [Required]
        [StringLength(255)]
        public string TenSP { get; set; } = string.Empty;

        public string? MoTaChiTiet { get; set; }
        public string? ThongTinKyThuat { get; set; }

        // --- FIX DANH MỤC ---
        [Required]
        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")] // Thêm dòng này để chắc chắn không lỗi giống MaNhaSX
        public virtual DanhMucSanPham? DanhMuc { get; set; }

        // --- FIX NHÀ SẢN XUẤT ---
        public int? MaNhaSX { get; set; }

        [ForeignKey("MaNhaSX")]
        public virtual NhaSanXuat? NhaSanXuat { get; set; }
        // LƯU Ý: Đã xóa dòng "public virtual NhaSanXuat? NhaSX" ở dưới

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? GiaGiamGia { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuongTonKho { get; set; } = 0;

        public string? HinhAnh { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        public decimal DanhGiaXepHang { get; set; } = 0;

        public int SoLuotDanhGia { get; set; } = 0;
        public int SoLuotXem { get; set; } = 0;
        public bool TrangThai { get; set; } = true;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; }

        // Navigation Collections
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<DanhGiaSanPham> DanhGias { get; set; } = new List<DanhGiaSanPham>();
        public virtual ICollection<ThongKeLuotXem> ThongKes { get; set; } = new List<ThongKeLuotXem>();
    }
}