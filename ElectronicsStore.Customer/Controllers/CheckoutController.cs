using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Service;
using ElectronicsStore.Customer.Service.Pricing;
using ElectronicsStore.Customer.Service.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicsStore.Customer.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;
        // SỬA: Dùng PricingStrategyFactory thay vì IPricingStrategy trực tiếp
        private readonly PricingStrategyFactory _pricingFactory; 
        private readonly PaymentFactory _paymentFactory;
        private readonly ILogger<CheckoutController> _logger;

        // Inject các dịch vụ qua Constructor
        public CheckoutController(
            ElectronicsStoreDbContext context,
            PricingStrategyFactory pricingFactory, // SỬA Ở ĐÂY
            PaymentFactory paymentFactory,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _pricingFactory = pricingFactory;      // SỬA Ở ĐÂY
            _paymentFactory = paymentFactory;
            _logger = logger;
        }

        // GET: Hiển thị trang thanh toán
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var cartItems = await _context.GioHangs
                .Include(g => g.SanPham)
                .Where(g => g.MaND == userId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var user = await _context.NguoiDungs.FindAsync(userId);

            decimal tongTienHang = cartItems.Sum(x => x.SoLuong * x.SanPham.GiaBan);
            
            // DÙNG FACTORY ĐỂ LẤY STRATEGY VÀ TÍNH TOÁN HIỂN THỊ LÊN VIEW
            var pricingStrategy = _pricingFactory.CreateStrategy(tongTienHang);
            decimal phiShip = pricingStrategy.CalculateShippingFee(tongTienHang);
            decimal tienGiamGia = pricingStrategy.CalculateDiscount(tongTienHang);

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TongTienHang = tongTienHang,
                PhiVanChuyen = phiShip,
                // Nếu CheckoutViewModel của bạn có thuộc tính TienGiamGia, hãy gán nó ở đây:
                // TienGiamGia = tienGiamGia, 
                HoTen = user?.TenDayDu,
                SoDienThoai = user?.SoDienThoai,
                Email = user?.Email,
                DiaChiCuThe = user?.DiaChiMacDinh
            };

            return View(model);
        }

        // POST: Xử lý đặt hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            _logger.LogInformation("\n================ BẮT ĐẦU XỬ LÝ ĐẶT HÀNG ================");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var cartItems = await _context.GioHangs
                .Include(g => g.SanPham)
                .Where(g => g.MaND == userId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // LOG: Thông tin khách hàng
            _logger.LogInformation($"[CONTROLLER] ---> Khách hàng ID: {userId} | Người nhận: {model.HoTen}");
            _logger.LogInformation($"[CONTROLLER] ---> Địa chỉ: {model.DiaChiCuThe}, {model.PhuongXa}, {model.QuanHuyen}, {model.TinhThanh}");

            // 1. BẢO MẬT: Tính lại tổng tiền từ Database
            decimal actualTongTienHang = cartItems.Sum(x => x.SoLuong * x.SanPham.GiaBan);
            _logger.LogInformation($"[CONTROLLER] ---> Tổng tiền trong giỏ: {actualTongTienHang:N0}đ");

            // 2. SỬ DỤNG PRICING FACTORY & STRATEGY
            var pricingStrategy = _pricingFactory.CreateStrategy(actualTongTienHang);
            _logger.LogInformation($"[FACTORY PATTERN] ---> Đã tự động phân loại và chọn chiến lược: {pricingStrategy.GetStrategyName()}");

            decimal phiShip = pricingStrategy.CalculateShippingFee(actualTongTienHang);
            decimal tienGiamGia = pricingStrategy.CalculateDiscount(actualTongTienHang);
            decimal tongThanhToan = actualTongTienHang + phiShip - tienGiamGia;

            _logger.LogInformation($"[STRATEGY PATTERN] ---> Kết quả tính toán:");
            _logger.LogInformation($"                     + Phí Ship: {phiShip:N0}đ");
            _logger.LogInformation($"                     + Tiền được giảm: -{tienGiamGia:N0}đ");
            _logger.LogInformation($"                     + TỔNG PHẢI TRẢ: {tongThanhToan:N0}đ");

            var order = new DonHang
            {
                MaDonHangGoc = DateTime.Now.Ticks.ToString(),
                MaND = userId,
                NgayTaoDon = DateTime.UtcNow,
                TongGiaTruocGiam = actualTongTienHang,
                PhiVanChuyen = phiShip,
                TienGiamGia = tienGiamGia,          // Đã thêm Tiền Giảm Giá
                TongGiaSauGiam = tongThanhToan,     // Tổng thanh toán thực tế
                TrangThaiDon = "Chờ xác nhận",
                TrangThaiThanhToan = "Chưa thanh toán",
                PhuongThucThanhToan = model.HinhThucThanhToan,
                DiaChiGiaoHang = $"{model.DiaChiCuThe}, {model.PhuongXa}, {model.QuanHuyen}, {model.TinhThanh}",
                ThanhPhoPhuong = $"{model.PhuongXa}, {model.QuanHuyen}, {model.TinhThanh}",
                SodTLienHe = model.SoDienThoai,
                GhiChu = model.GhiChu
            };

            _context.DonHangs.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[CONTROLLER] ---> Đã lưu Đơn hàng #{order.MaDH} thành công.");

            // Lưu chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDH = order.MaDH,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    GiaTaiThoiDiem = item.SanPham.GiaBan,
                    ThanhTien = item.SoLuong * item.SanPham.GiaBan
                });
            }

            // Ghi lịch sử đơn hàng
            _context.LichSuDonHangs.Add(new LichSuDonHang
            {
                MaDH = order.MaDH,
                TrangThaiCu = "",
                TrangThaiMoi = "Chờ xác nhận",
                NgayCapNhat = DateTime.UtcNow,
                LyDo = "Khách hàng tạo đơn mới",
                MaNguoiCapNhat = userId
            });

            // Xóa giỏ hàng
            _context.GioHangs.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // 3. SỬ DỤNG PAYMENT FACTORY: Tạo hình thức thanh toán
            _logger.LogInformation($"[FACTORY PATTERN] ---> Khách hàng chọn thanh toán qua: {model.HinhThucThanhToan}");
            var paymentStrategy = _paymentFactory.Create(model.HinhThucThanhToan);
            string redirectUrl = paymentStrategy.GeneratePaymentUrl(order, HttpContext);

            _logger.LogInformation($"[CONTROLLER] ---> Chuyển hướng tới URL: {redirectUrl}");
            _logger.LogInformation("================ KẾT THÚC XỬ LÝ ========================\n");

            return Redirect(redirectUrl);
        }

        // Callback VNPay
        public async Task<IActionResult> PaymentCallback()
        {
            var response = Request.Query;
            if (response.Count > 0)
            {
                string vnp_HashSecret = "7BGOEAARKIS3HSKTON23Q794R1EDMSXA";
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in response)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value);
                    }
                }

                string orderId = response["vnp_TxnRef"];
                string vnp_ResponseCode = response["vnp_ResponseCode"];
                string vnp_SecureHash = response["vnp_SecureHash"];
                long vnp_Amount = Convert.ToInt64(response["vnp_Amount"]) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        var order = await _context.DonHangs.FindAsync(int.Parse(orderId));
                        if (order != null)
                        {
                            order.TrangThaiThanhToan = "Đã thanh toán (VNPay)";
                            _context.LichSuThanhToans.Add(new LichSuThanhToan
                            {
                                MaDH = order.MaDH,
                                MaND = order.MaND,
                                SoTienThanhToan = vnp_Amount,
                                PhuongThuc = "VNPay",
                                TrangThaiGD = "Thành công",
                                MaThamChieu = vnp_SecureHash.Substring(0, Math.Min(20, vnp_SecureHash.Length)),
                                NgayThanhToan = DateTime.UtcNow
                            });
                            await _context.SaveChangesAsync();
                        }
                        return RedirectToAction("Success", new { orderId = orderId });
                    }
                }
            }
            return RedirectToAction("Failure");
        }

        public IActionResult Success(int? orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public IActionResult Failure() => View();
    }
}