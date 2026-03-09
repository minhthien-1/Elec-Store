using Supabase.Postgrest.Attributes;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;

namespace ElectronicsStore.API.Models
{
    [Table("SanPham")]
    public class Product : BaseModel
    {
        [PrimaryKey("id", false)] // Thêm false để báo là không tự tăng phía Client
        [JsonPropertyName("id")]  // Đảm bảo JSON trả về có tên là id
        public int Id { get; set; }

        [Column("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Column("price")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [Column("stock")]
        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [Column("category")]
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("HinhAnh")] 
        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }
    }
}