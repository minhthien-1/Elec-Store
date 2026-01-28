using System.ComponentModel.DataAnnotations;
namespace ElectronicsStore.API.Models.Entities
{
    public class DanhMucSanPham
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; }

        // Navigation
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();

    }
}
