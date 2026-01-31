namespace ElectronicsStore.Customer.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string? MoTaChiTiet { get; set; }
        public string? ThongTinKyThuat { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaGiamGia { get; set; }
        public int SoLuongTonKho { get; set; }
        public string? HinhAnh { get; set; }
        public decimal DanhGiaXepHang { get; set; }
        public int SoLuotDanhGia { get; set; }
        public int SoLuotXem { get; set; }

        public CategoryDto DanhMuc { get; set; }
        public BrandDto NhaSX { get; set; }
        public List<ReviewDto> DanhGias { get; set; }
        public List<ProductThumbnailDto> SanPhamTuongTu { get; set; } = new List<ProductThumbnailDto>();
    }

    public class ProductThumbnailDto
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
    }

    public class CategoryDto { public int MaDanhMuc { get; set; } public string TenDanhMuc { get; set; } }
    public class BrandDto { public int MaNhaSX { get; set; } public string TenNhaSX { get; set; } }
    public class ReviewDto
    {
        public int MaDG { get; set; }
        public int DemSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime ThemTrongDB { get; set; }
        public UserDto NguoiDung { get; set; }
    }
    public class UserDto { public string TenDayDu { get; set; } }
}