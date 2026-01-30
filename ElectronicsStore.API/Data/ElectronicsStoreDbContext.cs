using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Models.Entities;
namespace ElectronicsStore.API.Data
{
    public class ElectronicsStoreDbContext : DbContext
    {
        public ElectronicsStoreDbContext(DbContextOptions<ElectronicsStoreDbContext> options)
    : base(options)
        {
        }

        // DbSets
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
        public DbSet<LichSuDonHang> LichSuDonHangs { get; set; }
        public DbSet<LichSuThanhToan> LichSuThanhToans { get; set; }
        public DbSet<MaKhuyenMai> MaKhuyenMais { get; set; }
        public DbSet<NhaSanXuat> NhaSanXuats { get; set; }
        public DbSet<ThongKeLuotXem> ThongKeLuotXems { get; set; }

    }
}
