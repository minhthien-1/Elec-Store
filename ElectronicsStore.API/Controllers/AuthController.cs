using ElectronicsStore.API.Data;
using ElectronicsStore.API.Helpers;
using ElectronicsStore.API.Models;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ElectronicsStore.API.Services;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ElectronicsStoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ElectronicsStoreDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        #region Endpoints

        // ĐĂNG KÝ
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                //  Bước 1: Validate input
                var validationErrors = ValidationHelper.ValidateRegister(request);
                if (validationErrors.Count > 0)
                {
                    _logger.LogWarning($"Validation failed: {string.Join(", ", validationErrors)}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationErrors
                    });
                }

                // Bước 2: Kiểm tra email trùng (case-insensitive)
                var emailLower = request.Email.ToLower().Trim();
                var existingEmail = await _context.NguoiDungs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

                if (existingEmail != null)
                {
                    _logger.LogWarning($"Email already registered: {request.Email}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email đã được đăng ký"
                    });
                }

                // Bước 3: Kiểm tra số điện thoại trùng (nếu có)
                if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
                {
                    var existingPhone = await _context.NguoiDungs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.SoDienThoai == request.SoDienThoai);

                    if (existingPhone != null)
                    {
                        _logger.LogWarning($"Phone already registered: {request.SoDienThoai}");
                        return BadRequest(new
                        {
                            success = false,
                            message = "Số điện thoại đã được đăng ký"
                        });
                    }
                }

                // Bước 4: Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);

                //  Bước 5: Tạo user mới
                var nguoiDung = new NguoiDung
                {
                    Email = emailLower,
                    TenDayDu = request.TenDayDu.Trim(),
                    SoDienThoai = request.SoDienThoai?.Trim(),
                    MatKhauHash = hashedPassword,
                    DiaChiChiTiet = null,
                    ThanhPho = null,
                    QuocGia = "Việt Nam",
                    DiaChiMacDinh = null,
                    LaQuanTriVien = false,
                    DangHoatDong = true,
                    ThemTrongDB = DateTime.UtcNow,
                    SuaDoi = null
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully: {request.Email}");

                return CreatedAtAction(nameof(Register), new
                {
                    success = true,
                    message = "Đăng ký thành công! Vui lòng đăng nhập",
                    user = new
                    {
                        maNguoiDung = nguoiDung.MaND,
                        email = nguoiDung.Email,
                        tenDayDu = nguoiDung.TenDayDu,
                        soDienThoai = nguoiDung.SoDienThoai
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database error in Register: {dbEx.Message}");
                _logger.LogError($"Inner exception: {dbEx.InnerException?.Message}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi cơ sở dữ liệu",
                    details = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Register: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống",
                    details = ex.Message
                });
            }
        }

        // ĐĂNG NHẬP
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { success = false, message = "Email không được để trống" });

                if (string.IsNullOrWhiteSpace(request.MatKhau))
                    return BadRequest(new { success = false, message = "Mật khẩu không được để trống" });

                // Tìm user theo email (case-insensitive)
                var emailLower = request.Email.ToLower().Trim();
                var user = await _context.NguoiDungs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {request.Email}");
                    return Unauthorized(new { success = false, message = "Email hoặc mật khẩu không chính xác" });
                }

                // Kiểm tra tài khoản có hoạt động
                if (!user.DangHoatDong)
                {
                    _logger.LogWarning($"Login attempt with inactive account: {user.Email}");
                    return Unauthorized(new { success = false, message = "Tài khoản này đã bị khóa" });
                }

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash);
                if (!isPasswordValid)
                {
                    _logger.LogWarning($"Login attempt with wrong password: {request.Email}");
                    return Unauthorized(new { success = false, message = "Email hoặc mật khẩu không chính xác" });
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation($"User logged in successfully: {user.Email}");

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    user = new
                    {
                        maNguoiDung = user.MaND,
                        email = user.Email,
                        tenDayDu = user.TenDayDu,
                        soDienThoai = user.SoDienThoai,
                        laQuanTriVien = user.LaQuanTriVien
                    },
                    token = new
                    {
                        accessToken = token,
                        tokenType = "Bearer",
                        expiresIn = 3600,
                        refreshToken = refreshToken
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Login: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // REFRESH TOKEN 

        [HttpPost("refresh-token")]
        public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { success = false, message = "Refresh token không được để trống" });

                var principal = GetClaimsFromExpiredToken(request.AccessToken);
                var email = principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, message = "Token không hợp lệ" });

                var user = await _context.NguoiDungs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return Unauthorized(new { success = false, message = "Người dùng không tồn tại" });

                var newToken = GenerateJwtToken(user);

                _logger.LogInformation($"✅ Token refreshed for user: {user.Email}");

                return Ok(new
                {
                    success = true,
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
                _logger.LogError($"❌ Error in RefreshToken: {ex.Message}");
                return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc đã hết hạn" });
            }
        }

        // ĐĂNG XUẤT 
        [HttpPost("logout")]
        [Authorize]  // Thêm dòng này
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User không hợp lệ" });

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaND.ToString() == userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Đăng xuất thành công" });
            }
            catch
            {
                return Unauthorized(new { success = false, message = "Token không hợp lệ" });
            }
        }


        // ========== LẤY THÔNG TIN HIỆN TẠI ==========
        /// <summary>
        /// GET: /api/auth/me
        /// Lấy thông tin user hiện tại (cần Authorization)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, message = "Token không hợp lệ" });

                var user = await _context.NguoiDungs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return NotFound(new { success = false, message = "Người dùng không tồn tại" });

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        maNguoiDung = user.MaND,
                        email = user.Email,
                        tenDayDu = user.TenDayDu,
                        soDienThoai = user.SoDienThoai,
                        laQuanTriVien = user.LaQuanTriVien,
                        diaChiChiTiet = user.DiaChiChiTiet,
                        thanhPho = user.ThanhPho,
                        quocGia = user.QuocGia,
                        diaChiMacDinh = user.DiaChiMacDinh,
                        dangHoatDong = user.DangHoatDong
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetCurrentUser: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
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
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = false
                    }, out SecurityToken securityToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating token: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Request Classes

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string MatKhau { get; set; } = string.Empty;
        }

        public class RefreshTokenRequest
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }

        #endregion
    }
}
