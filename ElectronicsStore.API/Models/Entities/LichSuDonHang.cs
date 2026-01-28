using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.API.Models.Entities
{
    public class LichSuDonHang
    {
        [Key]
        public int MaLS { get; set; }
        public int MaDH { get; set; }

        [StringLength(50)]
        public string? TrangThaiCu { get; set; }

        [StringLength(50)]
        public string? TrangThaiMoi { get; set; }

        public int? MaNguoiCapNhat { get; set; }
        public string? LyDo { get; set; }
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Navigation
        public virtual DonHang? DonHang { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
