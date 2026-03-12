using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ElectronicsStore.API.Models
{
    // Đây là file dành RIÊNG cho Supabase Join bảng
    [Table("NhaSanXuat")]
    public class NhaSanXuatModel : BaseModel
    {
        [PrimaryKey("MaNhaSX", false)]
        public int MaNhaSX { get; set; }

        [Column("TenNhaSX")]
        public string TenNhaSX { get; set; } = string.Empty;
    }
}