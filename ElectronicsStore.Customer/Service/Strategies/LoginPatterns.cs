using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElectronicsStore.Customer.Patterns
{
    // 1. Dữ liệu truyền vào (Context)
    public class LoginContextDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Provider { get; set; } // "Local", "Google", "Facebook"
    }

    // 2. INTERFACE STRATEGY
    public interface ILoginStrategy
    {
        Task<NguoiDung> AuthenticateAsync(LoginContextDto contextDto);
    }

    // 3. CHIẾN LƯỢC 1: Xử lý Đăng nhập hệ thống (Email/Password)
    public class LocalLoginStrategy : ILoginStrategy
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<LocalLoginStrategy> _logger;

        public LocalLoginStrategy(ElectronicsStoreDbContext context, ILogger<LocalLoginStrategy> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NguoiDung> AuthenticateAsync(LoginContextDto contextDto)
        {
            _logger.LogInformation("[STRATEGY PATTERN] ---> Đang chạy thuật toán kiểm tra DB và băm mật khẩu BCrypt...");
            
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == contextDto.Email);
            if (user == null) return null;

            bool isPasswordValid = false;
            try { isPasswordValid = BCrypt.Net.BCrypt.Verify(contextDto.Password, user.MatKhauHash); }
            catch { if (user.MatKhauHash == contextDto.Password) isPasswordValid = true; }

            return isPasswordValid ? user : null;
        }
    }

    // 4. CHIẾN LƯỢC 2: Xử lý Đăng nhập Mạng xã hội (Dùng chung cho cả GG và FB)
    public class ExternalLoginStrategy : ILoginStrategy
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<ExternalLoginStrategy> _logger;

        public ExternalLoginStrategy(ElectronicsStoreDbContext context, ILogger<ExternalLoginStrategy> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NguoiDung> AuthenticateAsync(LoginContextDto contextDto)
        {
            _logger.LogInformation($"[STRATEGY PATTERN] ---> Đang chạy thuật toán kiểm tra tài khoản liên kết {contextDto.Provider}...");
            
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == contextDto.Email);

            if (user == null)
            {
                _logger.LogInformation($"[STRATEGY PATTERN] ---> Khách hàng mới! Đang tự động tạo tài khoản từ dữ liệu {contextDto.Provider}...");
                string randomPassword = Guid.NewGuid().ToString() + "A1@";
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                user = new NguoiDung
                {
                    Email = contextDto.Email,
                    TenDayDu = contextDto.FullName,
                    SoDienThoai = "", 
                    MatKhauHash = passwordHash, 
                    QuocGia = "Việt Nam",
                    LaQuanTriVien = false,
                    DangHoatDong = true,
                    ThemTrongDB = DateTime.UtcNow
                };
                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }

    // 5. FACTORY: Quyết định dùng chiến lược nào
    public class LoginStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoginStrategyFactory> _logger;

        public LoginStrategyFactory(IServiceProvider serviceProvider, ILogger<LoginStrategyFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public ILoginStrategy GetStrategy(string provider)
        {
            _logger.LogInformation($"[FACTORY PATTERN] ---> Đã tự động phân loại và xuất xưởng chiến lược: {provider}");
            
            if (provider == "Local")
                return _serviceProvider.GetRequiredService<LocalLoginStrategy>();
            
            // Google và Facebook đều dùng chung một cách thức xử lý External
            return _serviceProvider.GetRequiredService<ExternalLoginStrategy>();
        }
    }
}