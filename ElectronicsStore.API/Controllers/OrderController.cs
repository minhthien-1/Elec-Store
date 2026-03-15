using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Observers;
using ElectronicsStore.API.Commands;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly OrderSubject _subject;

        public OrderController(ElectronicsStoreDbContext context, OrderSubject subject)
        {
            _context = context;
            _subject = subject;
        }

        // GET: /api/order/{maND}
        [HttpGet("{maND}")]
        public async Task<ActionResult<object>> GetUserOrders(int maND)
        {
            try
            {
                var user = await _context.NguoiDungs.FindAsync(maND);
                if (user == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                var orders = await _context.DonHangs
                    .Where(d => d.MaND == maND)
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                    .Select(d => new
                    {
                        d.MaDH,
                        d.MaDonHangGoc,
                        d.NgayTaoDon,
                        d.TongGiaTruocGiam,
                        d.TienGiamGia,
                        d.TongGiaSauGiam,
                        d.TrangThaiDon,
                        d.TrangThaiThanhToan,
                        d.PhuongThucThanhToan,
                        d.DiaChiGiaoHang,
                        d.GhiChu,
                        ChiTiet = d.ChiTietDonHangs.Select(c => new
                        {
                            c.MaChiTiet,
                            c.MaSP,
                            c.SoLuong,
                            c.GiaTaiThoiDiem,
                            c.GiamGiaCungChiTiet,
                            c.ThanhTien,
                            SanPham = new { c.SanPham!.TenSP, c.SanPham.HinhAnh }
                        }).ToList()
                    })
                    .OrderByDescending(d => d.NgayTaoDon)
                    .ToListAsync();

                return Ok(new { total = orders.Count, data = orders });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/order/detail/{maDH}
        [HttpGet("detail/{maDH}")]
        public async Task<ActionResult<object>> GetOrderDetail(int maDH)
        {
            try
            {
                var order = await _context.DonHangs
                    .Where(d => d.MaDH == maDH)
                    .Include(d => d.NguoiDung)
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                    .Include(d => d.LichSuDonHangs)
                    .Include(d => d.LichSuThanhToans)
                    .FirstOrDefaultAsync();

                if (order == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                return Ok(new
                {
                    maDH = order.MaDH,
                    maDonHangGoc = order.MaDonHangGoc,
                    ngayTaoDon = order.NgayTaoDon,
                    tongGiaTruocGiam = order.TongGiaTruocGiam,
                    phiVanChuyen = order.PhiVanChuyen,
                    tienGiamGia = order.TienGiamGia,
                    tongGiaSauGiam = order.TongGiaSauGiam,
                    trangThaiDon = order.TrangThaiDon,
                    trangThaiThanhToan = order.TrangThaiThanhToan,
                    phuongThucThanhToan = order.PhuongThucThanhToan,
                    diaChiGiaoHang = order.DiaChiGiaoHang,
                    thanhPhoPhuong = order.ThanhPhoPhuong,
                    soDTLienHe = order.SodTLienHe,
                    ghiChu = order.GhiChu,
                    ngayCapNhat = order.NgayCapNhat,
                    nguoiDung = new
                    {
                        order.NguoiDung!.TenDayDu,
                        order.NguoiDung.Email,
                        order.NguoiDung.SoDienThoai
                    },
                    chiTiet = order.ChiTietDonHangs.Select(c => new
                    {
                        c.MaChiTiet,
                        c.MaSP,
                        c.SoLuong,
                        c.GiaTaiThoiDiem,
                        c.GiamGiaCungChiTiet,
                        c.ThanhTien,
                        sanPham = new
                        {
                            c.SanPham!.TenSP,
                            c.SanPham.HinhAnh
                        }
                    }).ToList(),
                    lichSuDon = order.LichSuDonHangs.Select(l => new
                    {
                        l.MaLS,
                        l.TrangThaiCu,
                        l.TrangThaiMoi,
                        l.LyDo,
                        l.NgayCapNhat
                    }).ToList(),
                    lichSuThanhToan = order.LichSuThanhToans.Select(l => new
                    {
                        l.MaLS,
                        l.SoTienThanhToan,
                        l.PhuongThuc,
                        l.TrangThaiGD,
                        l.NgayThanhToan
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/order/create
        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateOrder([FromBody] CreateOrderRequest request)
        {

            try
            {
                var user = await _context.NguoiDungs.FindAsync(request.MaND);
                if (user == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                if (request.ChiTiet == null || request.ChiTiet.Count == 0)
                    return BadRequest(new { message = "Đơn hàng phải có ít nhất 1 sản phẩm" });

                decimal tongGia = 0;
                var chiTietList = new List<ChiTietDonHang>();

                foreach (var item in request.ChiTiet)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.MaSP);
                    if (sanPham == null)
                        return NotFound(new { message = $"Sản phẩm {item.MaSP} không tồn tại" });

                    if (sanPham.SoLuongTonKho < item.SoLuong)
                        return BadRequest(new { message = $"Sản phẩm {sanPham.TenSP} không đủ số lượng" });

                    var giaBan = sanPham.GiaGiamGia ?? sanPham.GiaBan;
                    var thanhTien = giaBan * item.SoLuong;
                    tongGia += thanhTien;

                    var chiTiet = new ChiTietDonHang
                    {
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong,
                        GiaTaiThoiDiem = sanPham.GiaBan,
                        GiamGiaCungChiTiet = item.GiamGia ?? 0,
                        ThanhTien = thanhTien,
                        NgayThem = DateTime.UtcNow
                    };
                    chiTietList.Add(chiTiet);
                }

                // Apply promotion code if provided
                decimal tienGiamGia = 0;
                if (!string.IsNullOrWhiteSpace(request.MaKhuyenMai))
                {
                    var promo = await _context.MaKhuyenMais
                        .FirstOrDefaultAsync(p => p.MaCode.ToLower() == request.MaKhuyenMai.ToLower());

                    if (promo != null && promo.TrangThai && promo.NgayBatDau <= DateTime.UtcNow && promo.NgayKetThuc >= DateTime.UtcNow)
                    {
                        if (promo.KieuGiam == "PhanTram")
                            tienGiamGia = (tongGia * promo.GiaTriGiam.GetValueOrDefault()) / 100;
                        else
                            tienGiamGia = promo.GiaTriGiam.GetValueOrDefault();

                        if (promo.GiaTriGiamToiDa.HasValue && tienGiamGia > promo.GiaTriGiamToiDa)
                            tienGiamGia = promo.GiaTriGiamToiDa.Value;

                        promo.SoLuotSuDung += 1;
                        _context.MaKhuyenMais.Update(promo);
                    }
                }

                var maDonHang = $"DH-{DateTime.UtcNow:yyyyMMddHHmmss}";
                var donHang = new DonHang
                {
                    MaDonHangGoc = maDonHang,
                    MaND = request.MaND,
                    NgayTaoDon = DateTime.UtcNow,
                    TongGiaTruocGiam = tongGia,
                    PhiVanChuyen = request.PhiVanChuyen ?? 0,
                    TienGiamGia = tienGiamGia,
                    TongGiaSauGiam = tongGia - tienGiamGia + (request.PhiVanChuyen ?? 0),
                    TrangThaiDon = "Chờ xác nhận",
                    TrangThaiThanhToan = "Chưa thanh toán",
                    DiaChiGiaoHang = request.DiaChiGiaoHang,
                    ThanhPhoPhuong = request.ThanhPhoPhuong,
                    SodTLienHe = request.SoDTLienHe,
                    GhiChu = request.GhiChu,
                    NgayCapNhat = DateTime.UtcNow
                };

                donHang.ChiTietDonHangs = chiTietList;

                // add history
                var lichSu = new LichSuDonHang
                {
                    TrangThaiCu = null,
                    TrangThaiMoi = "Chờ xác nhận",
                    LyDo = "Đơn hàng vừa được tạo",
                    NgayCapNhat = DateTime.UtcNow
                };

                donHang.LichSuDonHangs = new List<LichSuDonHang> { lichSu };

                Console.WriteLine("===== ORDER CONTROLLER =====");
                Console.WriteLine($"Creating order for user {request.MaND}");

                var command = new CreateOrderCommand(_context, donHang, _subject);

                await command.ExecuteAsync();

                return Created($"api/order/detail/{donHang.MaDH}", new
                {
                    message = "Tạo đơn hàng thành công",
                    order = new
                    {
                        donHang.MaDH,
                        donHang.MaDonHangGoc,
                        donHang.TongGiaSauGiam,
                        donHang.TrangThaiDon
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/order/confirm/{maDH}
        [HttpPut("confirm/{maDH}")]
        public async Task<ActionResult<object>> ConfirmOrder(int maDH)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(maDH);
                if (donHang == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                var trangThaiCu = donHang.TrangThaiDon;
                donHang.TrangThaiDon = "Đã xác nhận";
                donHang.NgayCapNhat = DateTime.UtcNow;

                var lichSu = new LichSuDonHang
                {
                    MaDH = maDH,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = "Đã xác nhận",
                    LyDo = "Admin đã xác nhận đơn hàng",
                    NgayCapNhat = DateTime.UtcNow
                };

                _context.DonHangs.Update(donHang);
                _context.LichSuDonHangs.Add(lichSu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xác nhận đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/order/pay/{maDH}
        [HttpPut("pay/{maDH}")]
        public async Task<ActionResult<object>> PayOrder(int maDH, [FromBody] PayOrderRequest request)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(maDH);
                if (donHang == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                donHang.TrangThaiThanhToan = "Đã thanh toán";
                donHang.PhuongThucThanhToan = request.PhuongThucThanhToan;
                donHang.NgayCapNhat = DateTime.UtcNow;

                var lichSuTT = new LichSuThanhToan
                {
                    MaDH = maDH,
                    MaND = donHang.MaND,
                    SoTienThanhToan = donHang.TongGiaSauGiam,
                    PhuongThuc = request.PhuongThucThanhToan,
                    TrangThaiGD = "Thành công",
                    NgayThanhToan = DateTime.UtcNow
                };

                _context.DonHangs.Update(donHang);
                _context.LichSuThanhToans.Add(lichSuTT);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/order/cancel/{maDH}
        [HttpPut("cancel/{maDH}")]
        public async Task<ActionResult<object>> CancelOrder(int maDH, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(maDH);
                if (donHang == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                if (donHang.TrangThaiDon == "Đã giao" || donHang.TrangThaiDon == "Đã hủy")
                    return BadRequest(new { message = "Không thể hủy đơn hàng này" });

                var trangThaiCu = donHang.TrangThaiDon;
                donHang.TrangThaiDon = "Đã hủy";
                donHang.NgayCapNhat = DateTime.UtcNow;

                var lichSu = new LichSuDonHang
                {
                    MaDH = maDH,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = "Đã hủy",
                    LyDo = request.LyDoHuy,
                    NgayCapNhat = DateTime.UtcNow
                };

                _context.DonHangs.Update(donHang);
                _context.LichSuDonHangs.Add(lichSu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class CreateOrderRequest
        {
            public int MaND { get; set; }
            public List<ChiTietRequest> ChiTiet { get; set; } = new List<ChiTietRequest>();
            public string? MaKhuyenMai { get; set; }
            public decimal? PhiVanChuyen { get; set; }
            public string? DiaChiGiaoHang { get; set; }
            public string? ThanhPhoPhuong { get; set; }
            public string? SoDTLienHe { get; set; }
            public string? GhiChu { get; set; }
        }

        public class ChiTietRequest
        {
            public int MaSP { get; set; }
            public int SoLuong { get; set; }
            public decimal? GiamGia { get; set; }
        }

        public class PayOrderRequest
        {
            public string PhuongThucThanhToan { get; set; } = string.Empty;
        }

        public class CancelOrderRequest
        {
            public string LyDoHuy { get; set; } = string.Empty;
        }
    }
}