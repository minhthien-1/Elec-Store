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

        // --- KHAI BÁO CÁC BẢNG (DbSets) ---
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }
        public DbSet<NhaSanXuat> NhaSanXuats { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }
        public DbSet<MaKhuyenMai> MaKhuyenMais { get; set; }
        public DbSet<LichSuThanhToan> LichSuThanhToans { get; set; }
        public DbSet<LichSuDonHang> LichSuDonHangs { get; set; }
        public DbSet<ThongKeLuotXem> ThongKeLuotXems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Ánh xạ tên bảng (Để khớp chính xác với SQL Supabase của bạn)
            modelBuilder.Entity<SanPham>().ToTable("SanPham");
            modelBuilder.Entity<DanhMucSanPham>().ToTable("DanhMucSanPham");
            modelBuilder.Entity<NhaSanXuat>().ToTable("NhaSanXuat");
            modelBuilder.Entity<NguoiDung>().ToTable("NguoiDung");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHang");
            modelBuilder.Entity<GioHang>().ToTable("GioHang");
            modelBuilder.Entity<DanhGiaSanPham>().ToTable("DanhGiaSanPham");
            modelBuilder.Entity<MaKhuyenMai>().ToTable("MaKhuyenMai");
            modelBuilder.Entity<LichSuThanhToan>().ToTable("LichSuThanhToan");
            modelBuilder.Entity<LichSuDonHang>().ToTable("LichSuDonHang");
            modelBuilder.Entity<ThongKeLuotXem>().ToTable("ThongKeLuotXem");

            // 2. Cấu hình Khóa ngoại (Sửa lỗi: DanhMucMaDanhMuc does not exist)

            // --- Bảng Sản Phẩm ---
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.DanhMuc)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(s => s.MaDanhMuc);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NhaSanXuat)
                .WithMany(n => n.SanPhams)
                .HasForeignKey(s => s.MaNhaSX);

            // --- Bảng Giỏ Hàng ---
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.SanPham)
                .WithMany(s => s.GioHangs)
                .HasForeignKey(g => g.MaSP);

            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.NguoiDung)
                .WithMany(n => n.GioHangs)
                .HasForeignKey(g => g.MaND);

            // --- Bảng Đơn Hàng ---
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NguoiDung)
                .WithMany(n => n.DonHangs)
                .HasForeignKey(d => d.MaND);

            // --- Bảng Chi Tiết Đơn Hàng ---
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(c => c.MaDH);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.SanPham)
                .WithMany(s => s.ChiTietDonHangs)
                .HasForeignKey(c => c.MaSP);

            // --- Bảng Thống Kê Lượt Xem (Cái này bạn mới gửi nè) ---
            modelBuilder.Entity<ThongKeLuotXem>()
                .HasOne(t => t.SanPham)
                .WithMany(s => s.ThongKes)
                .HasForeignKey(t => t.MaSP);

            modelBuilder.Entity<ThongKeLuotXem>()
                .HasOne(t => t.NguoiDung)
                .WithMany() // Nếu NguoiDung không có ICollection<ThongKeLuotXem>
                .HasForeignKey(t => t.MaND);
        }
    }
}