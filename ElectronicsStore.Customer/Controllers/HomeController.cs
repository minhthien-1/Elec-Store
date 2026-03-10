using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models; // BẮT BUỘC: Dùng Model của dự án Customer
using System.Net.Http.Json;

namespace ElectronicsStore.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            // Đã sửa lại đường dẫn cho khớp 100% với ProductController (thêm https và /api/)
            _httpClient.BaseAddress = new Uri("http://localhost:5145/api/"); 
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // ĐÃ SỬA: Ép kiểu dữ liệu API trả về thành List<ProductViewModel>
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products");

                if (products == null || !products.Any())
                {
                    return Content("Kết nối API thành công nhưng không có dữ liệu!");
                }

                return View(products);
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi gọi API: " + ex.Message);
            }
        }
    }
}