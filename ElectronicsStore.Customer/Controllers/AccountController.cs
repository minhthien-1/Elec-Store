using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BCrypt.Net;
using ElectronicsStore.Customer.Patterns;
using Microsoft.Extensions.Logging;

namespace ElectronicsStore.Customer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly LoginStrategyFactory _loginFactory;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ElectronicsStoreDbContext context, LoginStrategyFactory loginFactory, ILogger<AccountController> logger)
        {
            _context = context;
            _loginFactory = loginFactory;
            _logger = logger;
        }

        // ==========================================
        // --- 1. ĐĂNG NHẬP THƯỜNG (LOCAL) ---
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            _logger.LogInformation("================ BẮT ĐẦU XỬ LÝ ĐĂNG NHẬP ================");
            _logger.LogInformation($"[CONTROLLER] ---> Phương thức: Đăng nhập truyền thống (Hệ thống)");
            _logger.LogInformation($"[CONTROLLER] ---> Email nhận được: {model.Email}");

            // Sử dụng Factory để lấy chiến lược
            var strategy = _loginFactory.GetStrategy("Local");
            
            // Thực thi chiến lược
            var user = await strategy.AuthenticateAsync(new LoginContextDto { Email = model.Email, Password = model.Password, Provider = "Local" });

            if (user != null)
            {
                _logger.LogInformation($"[CONTROLLER] ---> Kết quả: Đăng nhập THÀNH CÔNG. Khách hàng ID: {user.MaND} | {user.TenDayDu}");
                
                await SignInUserAsync(user);
                TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning($"[CONTROLLER] ---> Kết quả: THẤT BẠI. Sai email hoặc mật khẩu.");
            TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // --- 2. ĐĂNG KÝ (Giữ nguyên của bạn) ---
        // ==========================================
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

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new ElectronicsStore.API.Models.Entities.NguoiDung
            {
                Email = model.Email,
                TenDayDu = model.TenDayDu,
                SoDienThoai = model.SoDienThoai,
                MatKhauHash = passwordHash, 
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

        // ==========================================
        // --- 3. ĐĂNG XUẤT (Giữ nguyên của bạn) ---
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        // ========================================================
        // --- 4. CÁC ACTION XỬ LÝ ĐĂNG NHẬP MẠNG XÃ HỘI ---
        // ========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalResponse()
        {
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            
            if (!result.Succeeded)
            {
                TempData["Error"] = "Lỗi đăng nhập từ mạng xã hội.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy tên provider (Google hoặc Facebook) từ dữ liệu trả về
            var provider = result.Principal.Identities.FirstOrDefault()?.AuthenticationType ?? "External";
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email; 

            if (string.IsNullOrEmpty(email))
            {
                await HttpContext.SignOutAsync("ExternalCookie");
                TempData["Error"] = "Tài khoản của bạn không cung cấp Email hợp lệ. Vui lòng thử cách khác.";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation("================ BẮT ĐẦU XỬ LÝ ĐĂNG NHẬP ================");
            _logger.LogInformation($"[CONTROLLER] ---> Phương thức: Mạng xã hội ({provider})");
            _logger.LogInformation($"[CONTROLLER] ---> Email nhận được: {email}");

            // Sử dụng Factory để lấy chiến lược
            var strategy = _loginFactory.GetStrategy(provider);
            
            // Thực thi chiến lược
            var user = await strategy.AuthenticateAsync(new LoginContextDto { Email = email, FullName = name, Provider = provider });

            _logger.LogInformation($"[CONTROLLER] ---> Kết quả: Đăng nhập THÀNH CÔNG. Khách hàng ID: {user.MaND} | {user.TenDayDu}");

            // Cấp Cookie và dọn dẹp
            await SignInUserAsync(user);
            await HttpContext.SignOutAsync("ExternalCookie");

            TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // --- HÀM HỖ TRỢ DÙNG CHUNG ---
        // ==========================================
        private async Task SignInUserAsync(API.Models.Entities.NguoiDung user)
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
        }
    }
}