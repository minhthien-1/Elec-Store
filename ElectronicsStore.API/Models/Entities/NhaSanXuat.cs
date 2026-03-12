using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.API.Models.Entities
{
    [Table("NhaSanXuat")] // Đây là Table của EF Core
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

        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}