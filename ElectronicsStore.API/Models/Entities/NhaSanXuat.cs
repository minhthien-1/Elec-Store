using System.ComponentModel.DataAnnotations;
namespace ElectronicsStore.API.Models.Entities
{
    public class NhaSanXuat
    {
        [Key]
        public int MaNhaSX { get; set; }

        [Required]
        [StringLength(200)]
        public string TenNhaSX { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DatNuoc { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        public string? Website { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;
        public DateTime? SuaDoi { get; set; }

        // Navigation
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
