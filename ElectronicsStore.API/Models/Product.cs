using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.API.Models;

// 1. Phải có attribute Table và viết ĐÚNG CHỮ HOA THƯỜNG như trong SQL
[Table("SanPham")]
public class Product : BaseModel
{
    // 2. Phải có attribute Column cho từng trường
    [PrimaryKey("MaSP", false)] // "MaSP" phải giống hệt trong SQL
    public int MaSP { get; set; }

    [Column("TenSP")]
    public string TenSP { get; set; } = string.Empty;

    [Column("MoTaChiTiet")]
    public string? MoTaChiTiet { get; set; }

    [Column("ThongTinKyThuat")]
    public string? ThongTinKyThuat { get; set; }

    [Column("MaDanhMuc")]
    public int MaDanhMuc { get; set; }

    [Column("MaNhaSX")]
    public int? MaNhaSX { get; set; }

    [Column("GiaBan")]
    public decimal GiaBan { get; set; }

    [Column("GiaGiamGia")]
    public decimal? GiaGiamGia { get; set; }

    [Column("SoLuongTonKho")]
    public int SoLuongTonKho { get; set; }

    [Column("HinhAnh")]
    public string? HinhAnh { get; set; }

    [Column("DanhGiaXepHang")]
    public decimal DanhGiaXepHang { get; set; }

    [Column("SoLuotDanhGia")]
    public int SoLuotDanhGia { get; set; }

    [Column("SoLuotXem")]
    public int SoLuotXem { get; set; }
    
    // Thuộc tính này sẽ tự động Join bảng nhờ Supabase
    [Reference(typeof(NhaSanXuatModel))] 
    public NhaSanXuatModel? NhaSanXuat { get; set; }
}