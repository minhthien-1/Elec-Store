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
                // Chuẩn bị dữ liệu gửi đến API
                var loginData = new
                {
                    email = email,
                    password = password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi API đăng nhập (thay đổi URL theo API của bạn)
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/auth/login";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result);

                    // Lưu token vào session
                    HttpContext.Session.SetString("AdminToken", loginResponse.Token);
                    HttpContext.Session.SetString("AdminEmail", email);

                    TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Product");
                }
                else
                {
                    ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Login");
        }
    }

    // Model để nhận response từ API
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}