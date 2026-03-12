namespace ElectronicsStore.Customer.Models
{
    // Lớp vỏ bọc khớp 100% với JSON API trả về (có chữ total và data)
    public class CategoryApiResponse
    {
        public int total { get; set; } 
        public List<CategoryViewModel> data { get; set; } = new(); 
    }

    public class CategoryViewModel
    {
        public int maDanhMuc { get; set; }
        public string tenDanhMuc { get; set; } = string.Empty;
        public int soSanPham { get; set; } 
        public string? hinhAnh { get; set; }
    }

    public class BrandViewModel
    {
        public int maNhaSX { get; set; }
        public string tenNhaSX { get; set; } = string.Empty;
    }
}