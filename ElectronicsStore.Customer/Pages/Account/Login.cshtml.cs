using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Patterns;
using ElectronicsStore.API.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ElectronicsStore.Customer.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly LoginStrategyFactory _loginFactory;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ElectronicsStoreDbContext context, LoginStrategyFactory loginFactory, ILogger<LoginModel> logger)
        {
            _context = context;
            _loginFactory = loginFactory;
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _logger.LogInformation("================ BẮT ĐẦU XỬ LÝ ĐĂNG NHẬP (RAZOR PAGE) ================");
            _logger.LogInformation($"[PAGE] ---> Phương thức: Đăng nhập truyền thống (Hệ thống)");
            _logger.LogInformation($"[PAGE] ---> Email nhận được: {Input.Email}");

            var strategy = _loginFactory.GetStrategy("Local");
            var user = await strategy.AuthenticateAsync(new LoginContextDto 
            { 
                Email = Input.Email, 
                Password = Input.Password, 
                Provider = "Local" 
            });

            if (user != null)
            {
                _logger.LogInformation($"[PAGE] ---> Kết quả: Đăng nhập THÀNH CÔNG. Khách hàng ID: {user.MaND} | {user.TenDayDu}");
                
                await SignInUserAsync(user);
                TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning($"[PAGE] ---> Kết quả: THẤT BẠI. Sai email hoặc mật khẩu.");
            TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
            return Page();
        }

        public IActionResult OnPostExternalLogin(string provider)
        {
            var redirectUrl = Url.Page("/Account/Login", "ExternalResponse");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> OnGetExternalResponseAsync()
        {
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            
            if (!result.Succeeded)
            {
                TempData["Error"] = "Lỗi đăng nhập từ mạng xã hội.";
                return RedirectToPage("/Account/Login");
            }

            var provider = result.Principal.Identities.FirstOrDefault()?.AuthenticationType ?? "External";
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email; 

            if (string.IsNullOrEmpty(email))
            {
                await HttpContext.SignOutAsync("ExternalCookie");
                TempData["Error"] = "Tài khoản của bạn không cung cấp Email hợp lệ. Vui lòng thử cách khác.";
                return RedirectToPage("/Account/Login");
            }

            _logger.LogInformation("================ BẮT ĐẦU XỬ LÝ ĐĂNG NHẬP MẠNG XÃ HỘI ================");
            _logger.LogInformation($"[PAGE] ---> Phương thức: Mạng xã hội ({provider})");
            _logger.LogInformation($"[PAGE] ---> Email nhận được: {email}");

            var strategy = _loginFactory.GetStrategy(provider);
            var user = await strategy.AuthenticateAsync(new LoginContextDto { Email = email, FullName = name, Provider = provider });

            _logger.LogInformation($"[PAGE] ---> Kết quả: Đăng nhập THÀNH CÔNG. Khách hàng ID: {user.MaND} | {user.TenDayDu}");

            await SignInUserAsync(user);
            await HttpContext.SignOutAsync("ExternalCookie");

            TempData["Success"] = $"Chào mừng {user.TenDayDu}!";
            return RedirectToAction("Index", "Home");
        }

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
