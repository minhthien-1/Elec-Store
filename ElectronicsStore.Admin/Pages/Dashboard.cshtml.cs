using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Admin.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ElectronicsStoreDbContext context, ILogger<DashboardModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── KPI Cards ──────────────────────────────────────────────────
        public int TongSanPham { get; set; }
        public int TongDonHang { get; set; }
        public int TongNguoiDung { get; set; }
        public decimal DoanhThuThang { get; set; }   // tháng hiện tại
        public decimal DoanhThuTongCong { get; set; }

        // ── Thống kê đơn hàng theo trạng thái ──────────────────────────
        public int DonChoXacNhan { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonHuy { get; set; }

        // ── Biểu đồ doanh thu 6 tháng gần nhất ────────────────────────
        // Dạng: [{ Thang = "01/2026", DoanhThu = 5000000 }, ...]
        public List<DoanhThuTheoThang> DoanhThu6Thang { get; set; } = new();

        // ── Đơn hàng gần đây (10 đơn mới nhất) ────────────────────────
        public List<DonHangGanDay> DonHangGanDays { get; set; } = new();

        // ── Sản phẩm bán chạy (top 5) ──────────────────────────────────
        public List<SanPhamBanChay> TopSanPham { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra đăng nhập qua Session
            var token = HttpContext.Session.GetString("AdminToken");
            if (string.IsNullOrEmpty(token))
                return Redirect("/Login");

            var now = DateTime.UtcNow;
            var dauThang = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // ── Truy vấn song song để tăng tốc ─────────────────────────
            TongSanPham = await _context.SanPhams.CountAsync();
            TongDonHang = await _context.DonHangs.CountAsync();
            TongNguoiDung = await _context.NguoiDungs.CountAsync(n => !n.LaQuanTriVien);

            DoanhThuThang = await _context.DonHangs
                .Where(d => d.NgayTaoDon >= dauThang && d.TrangThaiDon != "Đã hủy")
                .SumAsync(d => (decimal?)d.TongGiaSauGiam) ?? 0;

            DoanhThuTongCong = await _context.DonHangs
                .Where(d => d.TrangThaiDon != "Đã hủy")
                .SumAsync(d => (decimal?)d.TongGiaSauGiam) ?? 0;

            // ── Đơn hàng theo trạng thái ────────────────────────────────
            DonChoXacNhan = await _context.DonHangs.CountAsync(d => d.TrangThaiDon == "Chờ xác nhận");
            DonDangGiao   = await _context.DonHangs.CountAsync(d => d.TrangThaiDon == "Đang giao");
            DonHoanThanh  = await _context.DonHangs.CountAsync(d => d.TrangThaiDon == "Hoàn thành");
            DonHuy        = await _context.DonHangs.CountAsync(d => d.TrangThaiDon == "Đã hủy");

            // ── Doanh thu 6 tháng gần nhất ──────────────────────────────
            var sau_thang_truoc = now.AddMonths(-5);
            var dauSauThang = new DateTime(sau_thang_truoc.Year, sau_thang_truoc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var rawData = await _context.DonHangs
                .Where(d => d.NgayTaoDon >= dauSauThang && d.TrangThaiDon != "Đã hủy")
                .GroupBy(d => new { d.NgayTaoDon.Year, d.NgayTaoDon.Month })
                .Select(g => new
                {
                    Nam  = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(d => d.TongGiaSauGiam)
                })
                .OrderBy(g => g.Nam).ThenBy(g => g.Thang)
                .ToListAsync();

            DoanhThu6Thang = rawData.Select(r => new DoanhThuTheoThang
            {
                Thang    = $"{r.Thang:D2}/{r.Nam}",
                DoanhThu = r.DoanhThu
            }).ToList();

            // ── Đơn hàng gần đây ────────────────────────────────────────
            DonHangGanDays = await _context.DonHangs
                .Include(d => d.NguoiDung)
                .OrderByDescending(d => d.NgayTaoDon)
                .Take(8)
                .Select(d => new DonHangGanDay
                {
                    MaDH            = d.MaDH,
                    MaDonHangGoc    = d.MaDonHangGoc,
                    TenKhachHang    = d.NguoiDung != null ? d.NguoiDung.TenDayDu : "Khách vãng lai",
                    NgayTao         = d.NgayTaoDon,
                    TongTien        = d.TongGiaSauGiam,
                    TrangThaiDon    = d.TrangThaiDon,
                    TrangThaiTT     = d.TrangThaiThanhToan
                })
                .ToListAsync();

            // ── Top sản phẩm bán chạy ────────────────────────────────────
            TopSanPham = await _context.ChiTietDonHangs
                .Include(c => c.SanPham)
                .GroupBy(c => new { c.MaSP, c.SanPham!.TenSP })
                .Select(g => new SanPhamBanChay
                {
                    MaSP    = g.Key.MaSP,
                    TenSP   = g.Key.TenSP,
                    SoLuong = g.Sum(c => c.SoLuong),
                    DoanhThu = g.Sum(c => c.ThanhTien)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }

    // ── DTOs nội bộ ────────────────────────────────────────────────────
    public class DoanhThuTheoThang
    {
        public string Thang { get; set; } = "";
        public decimal DoanhThu { get; set; }
    }

    public class DonHangGanDay
    {
        public int MaDH { get; set; }
        public string MaDonHangGoc { get; set; } = "";
        public string TenKhachHang { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiDon { get; set; } = "";
        public string TrangThaiTT { get; set; } = "";
    }

    public class SanPhamBanChay
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
