using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace ElectronicsStore.Customer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public AccountController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // --- ĐĂNG NHẬP (SESSION COOKIE) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            // Tìm người dùng trong database
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.MatKhauHash == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.TenDayDu),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.MaND.ToString()),
                    new Claim(ClaimTypes.Role, user.LaQuanTriVien ? "Admin" : "Customer")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Cấu hình không ghi nhớ (IsPersistent = false) để reset khi tắt trình duyệt
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Không lưu vĩnh viễn
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
            return RedirectToAction("Index", "Home");
        }

        // --- ĐĂNG KÝ (POSTGRESQL COMPATIBLE) ---
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

            var newUser = new NguoiDung
            {
                Email = model.Email,
                TenDayDu = model.TenDayDu,
                SoDienThoai = model.SoDienThoai,
                MatKhauHash = model.Password,
                QuocGia = "Việt Nam",
                LaQuanTriVien = false,
                DangHoatDong = true,
                ThemTrongDB = DateTime.UtcNow // PostgreSQL yêu cầu chuẩn UTC
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

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }
    }
}
