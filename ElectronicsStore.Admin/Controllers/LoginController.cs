using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ElectronicsStore.Admin.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập thì chuyển đến Dashboard
            if (HttpContext.Session.GetString("AdminToken") != null)
            {
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            try
            {
                // 1. Chuẩn bị dữ liệu gửi đến API (Phải khớp với LoginRequest ở API)
                var loginData = new
                {
                    Email = email,
                    MatKhau = password // Ghi đúng tên thuộc tính theo LoginRequest của API
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 2. Gọi API đăng nhập (URL lấy từ appsettings.json của Admin)
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/auth/admin-login";
                var response = await _httpClient.PostAsync(apiUrl, content);

                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(resultString);
                    var root = doc.RootElement;

                    if (root.GetProperty("success").GetBoolean())
                    {
                        var user = root.GetProperty("user");
                        var tokenObj = root.GetProperty("token");

                        string accessToken = tokenObj.GetProperty("accessToken").GetString();
                        bool isQuanTri = user.GetProperty("laQuanTriVien").GetBoolean();

                        if (isQuanTri)
                        {
                            HttpContext.Session.SetString("AdminToken", accessToken);
                            HttpContext.Session.SetString("AdminName", user.GetProperty("tenDayDu").GetString());

                            return RedirectToAction("Index", "Product");
                        }
                    }
                }

                // Nếu không thành công, cố gắng lấy message từ API để debug
                try
                {
                    using var doc = JsonDocument.Parse(resultString);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("message", out var msg))
                    {
                        ViewBag.ErrorMessage = msg.GetString();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = $"Lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}";
                    }
                }
                catch
                {
                    ViewBag.ErrorMessage = $"Lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi kết nối đến Server: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch session khi đăng xuất
            return RedirectToAction("Index");
        }
    }

}