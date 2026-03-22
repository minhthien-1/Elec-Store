using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ElectronicsStore.Customer.Models
{
    [Table("NguoiDung")]
    public class NguoiDung : BaseModel
    {
        [PrimaryKey("MaND", false)]
        public int MaND { get; set; }

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("TenDayDu")]
        public string TenDayDu { get; set; } = string.Empty;

        [Column("SoDienThoai")]
        public string? SoDienThoai { get; set; } // SQL cho phép null

        [Column("MatKhauHash")]
        public string MatKhauHash { get; set; } = string.Empty;

        [Column("DiaChiChiTiet")]
        public string? DiaChiChiTiet { get; set; } // SQL cho phép null

        [Column("ThanhPho")]
        public string? ThanhPho { get; set; } // SQL cho phép null

        [Column("QuocGia")]
        public string? QuocGia { get; set; } = "Việt Nam";

        // CỘT NÀY QUAN TRỌNG: Để sửa lỗi "NguoiDung does not contain a definition for DiaChiMacDinh"
        [Column("DiaChiMacDinh")]
        public string? DiaChiMacDinh { get; set; }

        [Column("LaQuanTriVien")]
        public bool LaQuanTriVien { get; set; } = false;

        [Column("DangHoatDong")]
        public bool DangHoatDong { get; set; } = true;

        [Column("ThemTrongDB")]
        public DateTime ThemTrongDB { get; set; } = DateTime.Now;

        [Column("SuaDoi")]
        public DateTime? SuaDoi { get; set; }
    }
}