using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
// Thêm namespace này
using BCrypt.Net;
using NguoiDung = ElectronicsStore.Customer.Models.NguoiDung;

namespace ElectronicsStore.Customer.Controllers
{
    public class AccountController : Controller
    {

        private readonly ElectronicsStoreDbContext _context;

        public AccountController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // --- ĐĂNG NHẬP (CẬP NHẬT KIỂM TRA MẬT KHẨU BĂM) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            // 1. Tìm user theo Email trước (KHÔNG kiểm tra password trong câu truy vấn này)
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // 2. Kiểm tra User có tồn tại không VÀ Check mật khẩu
            if (user != null)
            {
                // Dùng BCrypt để so sánh mật khẩu nhập vào với mật khẩu đã mã hóa trong DB
                // Lưu ý: Nếu user cũ trong DB chưa mã hóa, hàm này sẽ trả về false -> Cần xử lý riêng hoặc reset DB
                bool isPasswordValid = false;

                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.MatKhauHash);
                }
                catch
                {
                    // Fallback: Nếu verify lỗi (do DB cũ lưu plain text), so sánh thường
                    if (user.MatKhauHash == model.Password) isPasswordValid = true;
                }

                if (isPasswordValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.TenDayDu),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("UserId", user.MaND.ToString()),
                        new Claim(ClaimTypes.Role, user.LaQuanTriVien ? "Admin" : "Customer")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
            return RedirectToAction("Index", "Home");
        }

        // --- ĐĂNG KÝ (CẬP NHẬT MÃ HÓA MẬT KHẨU) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            var emailExists = await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                TempData["Error"] = "Email này đã được sử dụng!";
                return RedirectToAction("Index", "Home");
            }

            // MÃ HÓA MẬT KHẨU TẠI ĐÂY
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new ElectronicsStore.API.Models.Entities.NguoiDung
            {
                Email = model.Email,
                TenDayDu = model.TenDayDu,
                SoDienThoai = model.SoDienThoai,
                MatKhauHash = passwordHash, // Lưu chuỗi đã mã hóa
                QuocGia = "Việt Nam",
                LaQuanTriVien = false,
                DangHoatDong = true,
                ThemTrongDB = DateTime.UtcNow
            };

            try
            {
                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập ngay.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi lưu dữ liệu: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // --- ĐĂNG XUẤT (Giữ nguyên) ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }
    }
}