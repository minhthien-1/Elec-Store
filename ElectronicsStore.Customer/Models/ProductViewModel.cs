namespace ElectronicsStore.Customer.Models 
{
    public class ProductViewModel
    {
        public int maSP { get; set; }
        public string tenSP { get; set; } = string.Empty;
        public decimal giaBan { get; set; }
        public int soLuongTonKho { get; set; }
        public int maDanhMuc { get; set; }
        public string? hinhAnh { get; set; }
        public decimal? giaGiamGia { get; set; } 
        public string? moTa { get; set; }

        // Bắt buộc phải có thuộc tính này để hứng Thương hiệu từ API
        public BrandViewModel? nhaSanXuat { get; set; } 
    }
}