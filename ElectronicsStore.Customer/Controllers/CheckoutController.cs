using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Customer.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public CheckoutController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị trang thanh toán
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var cartItems = await _context.GioHangs.Include(g => g.SanPham).Where(g => g.MaND == userId).ToListAsync();
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var user = await _context.NguoiDungs.FindAsync(userId);

            decimal tongTienHang = cartItems.Sum(x => x.SoLuong * x.SanPham.GiaBan);
            decimal phiShip = tongTienHang >= 1000000 ? 0 : 30000;

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TongTienHang = tongTienHang,
                PhiVanChuyen = phiShip,
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
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var cartItems = await _context.GioHangs.Include(g => g.SanPham).Where(g => g.MaND == userId).ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // 1. Tạo Đơn Hàng Mới
            var order = new DonHang
            {
                MaDonHangGoc = DateTime.Now.Ticks.ToString(), // Mã tạm
                MaND = userId,
                NgayTaoDon = DateTime.UtcNow,
                TongGiaTruocGiam = model.TongTienHang,
                PhiVanChuyen = model.TongTienHang >= 1000000 ? 0 : 30000,
                TongGiaSauGiam = model.TongThanhToan,
                TrangThaiDon = "Chờ xác nhận",
                TrangThaiThanhToan = "Chưa thanh toán",
                PhuongThucThanhToan = model.HinhThucThanhToan,
                // Ghép địa chỉ đầy đủ
                DiaChiGiaoHang = $"{model.DiaChiCuThe}, {model.PhuongXa}, {model.QuanHuyen}, {model.TinhThanh}",
                ThanhPhoPhuong = $"{model.PhuongXa}, {model.QuanHuyen}, {model.TinhThanh}", // Lưu phần hành chính riêng
                SodTLienHe = model.SoDienThoai,
                GhiChu = model.GhiChu
            };

            _context.DonHangs.Add(order);
            await _context.SaveChangesAsync(); // Lưu để sinh MaDH

            // 2. Lưu Chi Tiết Đơn Hàng
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

            // 3. Ghi Lịch Sử Đơn Hàng (Bắt đầu)
            _context.LichSuDonHangs.Add(new LichSuDonHang
            {
                MaDH = order.MaDH,
                TrangThaiCu = "",
                TrangThaiMoi = "Chờ xác nhận",
                NgayCapNhat = DateTime.UtcNow,
                LyDo = "Khách hàng tạo đơn mới",
                MaNguoiCapNhat = userId // Khách tự tạo
            });

            // 4. Xóa Giỏ hàng
            _context.GioHangs.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // 5. Điều hướng thanh toán
            if (model.HinhThucThanhToan == "VNPAY")
            {
                string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                string vnp_TmnCode = "WYBQPSHN";
                string vnp_HashSecret = "QK4DHXARPHOYTV1R3E3HU176A6QIDMAH";

                // QUAN TRỌNG: Sửa PORT 44371 thành port máy bạn
                string vnp_Returnurl = "https://localhost:44371/Checkout/PaymentCallback";

                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)order.TongGiaSauGiam * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.MaDH);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", order.MaDH.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return Redirect(paymentUrl);
            }

            // SỬA: Truyền orderId sang trang Success cho trường hợp COD
            return RedirectToAction("Success", new { orderId = order.MaDH });
        }

        // Callback VNPay
        public async Task<IActionResult> PaymentCallback()
        {
            var response = Request.Query;
            if (response.Count > 0)
            {
                string vnp_HashSecret = "QK4DHXARPHOYTV1R3E3HU176A6QIDMAH";
                var vnpayData = response;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value);
                    }
                }

                string orderId = vnpayData["vnp_TxnRef"];
                string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
                string vnp_SecureHash = vnpayData["vnp_SecureHash"];
                long vnp_Amount = Convert.ToInt64(vnpayData["vnp_Amount"]) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00") // Thành công
                    {
                        var order = await _context.DonHangs.FindAsync(int.Parse(orderId));
                        if (order != null)
                        {
                            // Cập nhật trạng thái
                            order.TrangThaiThanhToan = "Đã thanh toán (VNPay)";

                            // Ghi lịch sử thanh toán (Quan trọng)
                            _context.LichSuThanhToans.Add(new LichSuThanhToan
                            {
                                MaDH = order.MaDH,
                                MaND = order.MaND,
                                SoTienThanhToan = vnp_Amount,
                                PhuongThuc = "VNPay",
                                TrangThaiGD = "Thành công",
                                MaThamChieu = vnp_SecureHash.Substring(0, 20), // Lưu một phần mã hash làm tham chiếu
                                NgayThanhToan = DateTime.UtcNow
                            });

                            await _context.SaveChangesAsync();
                        }
                        // SỬA: Truyền orderId sang trang Success
                        return RedirectToAction("Success", new { orderId = orderId });
                    }
                    else // Thất bại
                    {
                        // Ghi log thất bại nếu cần (Tùy chọn)
                        return View("Failure");
                    }
                }
            }
            return View("Failure");
        }

        // SỬA: Nhận tham số orderId và đẩy vào ViewBag
        public IActionResult Success(int? orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public IActionResult Failure() => View();
    }
}