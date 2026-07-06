using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicsStore.Customer.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(ElectronicsStoreDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var emailExists = await _context.NguoiDungs.AnyAsync(u => u.Email == Input.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Input.Email", "Email này đã được sử dụng!");
                TempData["Error"] = "Email này đã được sử dụng!";
                return Page();
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password);

            var newUser = new ElectronicsStore.API.Models.Entities.NguoiDung
            {
                Email = Input.Email,
                TenDayDu = Input.TenDayDu,
                SoDienThoai = Input.SoDienThoai,
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
                TempData["Success"] = "Đăng ký thành công! Đăng nhập ngay.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng ký: {ex.Message}");
                TempData["Error"] = "Lỗi lưu dữ liệu: " + ex.Message;
                return Page();
            }
        }
    }
}
