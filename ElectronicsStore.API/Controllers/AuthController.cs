using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ElectronicsStoreDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email không được để trống" });

                if (string.IsNullOrWhiteSpace(request.TenDayDu))
                    return BadRequest(new { message = "Tên đầy đủ không được để trống" });

                if (string.IsNullOrWhiteSpace(request.MatKhau) || request.MatKhau.Length < 6)
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự" });

                // Check if email already exists
                var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (existingUser != null)
                    return BadRequest(new { message = "Email này đã được đăng ký" });

                // Hash password
                var passwordHash = HashPassword(request.MatKhau);

                var nguoiDung = new NguoiDung
                {
                    Email = request.Email.ToLower(),
                    TenDayDu = request.TenDayDu,
                    SoDienThoai = request.SoDienThoai,
                    MatKhauHash = passwordHash,
                    LaQuanTriVien = false, // Default: Customer
                    DangHoatDong = true,
                    ThemTrongDB = DateTime.Now
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New user registered: {request.Email}");

                return Created($"api/auth/user/{nguoiDung.MaND}", new
                {
                    message = "Đăng ký thành công! Vui lòng đăng nhập",
                    user = new
                    {
                        maNguoiDung = nguoiDung.MaND,
                        email = nguoiDung.Email,
                        tenDayDu = nguoiDung.TenDayDu
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Register: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email không được để trống" });

                if (string.IsNullOrWhiteSpace(request.MatKhau))
                    return BadRequest(new { message = "Mật khẩu không được để trống" });

                // Find user by email
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (user == null)
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác" });

                // Check if user is active
                if (!user.DangHoatDong)
                    return Unauthorized(new { message = "Tài khoản này đã bị khóa" });

                // Verify password
                if (!VerifyPassword(request.MatKhau, user.MatKhauHash))
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác" });

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token (optional - for token refresh functionality)
                // You can store this in database if needed

                _logger.LogInformation($"User logged in: {user.Email}");

                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    user = new
                    {
                        maNguoiDung = user.MaND,
                        email = user.Email,
                        tenDayDu = user.TenDayDu,
                        laQuanTriVien = user.LaQuanTriVien
                    },
                    token = new
                    {
                        accessToken = token,
                        tokenType = "Bearer",
                        expiresIn = 3600, // 1 hour
                        refreshToken = refreshToken
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Login: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { message = "Refresh token không được để trống" });

                // In production, verify refresh token from database
                // For now, we'll just generate a new access token

                var principal = GetClaimsFromExpiredToken(request.AccessToken);
                var email = principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return Unauthorized(new { message = "Người dùng không tồn tại" });

                var newToken = GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Token đã được làm mới",
                    token = new
                    {
                        accessToken = newToken,
                        tokenType = "Bearer",
                        expiresIn = 3600
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RefreshToken: {ex.Message}");
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            }
        }

        // POST: /api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public ActionResult<object> Logout()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                _logger.LogInformation($"User logged out: {email}");

                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Logout: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/auth/me (Get current user info)
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                return Ok(new
                {
                    maNguoiDung = user.MaND,
                    email = user.Email,
                    tenDayDu = user.TenDayDu,
                    soDienThoai = user.SoDienThoai,
                    laQuanTriVien = user.LaQuanTriVien,
                    diaChiChiTiet = user.DiaChiChiTiet,
                    thanhPho = user.ThanhPho,
                    quocGia = user.QuocGia,
                    dangHoatDong = user.DangHoatDong
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCurrentUser: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        #region Helper Methods

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                var parts = hash.Split(':');
                if (parts.Length != 2)
                    return false;

                var salt = Convert.FromBase64String(parts[0]);
                var hashBytes = Convert.FromBase64String(parts[1]);

                using (var sha256 = SHA256.Create())
                {
                    var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
                    return computedHash.SequenceEqual(hashBytes);
                }
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaND.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.TenDayDu),
                new Claim(ClaimTypes.Role, user.LaQuanTriVien ? "Admin" : "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal? GetClaimsFromExpiredToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = false // Don't validate lifetime for refresh
                }, out SecurityToken securityToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Request Classes

        public class RegisterRequest
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên phải từ 3-150 ký tự")]
            public string TenDayDu { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string? SoDienThoai { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
            public string MatKhau { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            public string MatKhau { get; set; } = string.Empty;
        }

        public class RefreshTokenRequest
        {
            [Required]
            public string AccessToken { get; set; } = string.Empty;

            [Required]
            public string RefreshToken { get; set; } = string.Empty;
        }

        #endregion
    }
}