using ElectronicsStore.API.Models; // Lưu ý: Dùng Model chung hoặc khai báo lại
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ElectronicsStore.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        // Tiêm HttpClient vào thay vì DbContext
        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            // Cổng port phải khớp với project API đang chạy
            _httpClient.BaseAddress = new Uri("http://localhost:5145/"); 
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Gọi sang API thay vì dùng _context
                // Terminal bên project API sẽ nhảy log khi hàm này chạy
                var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products");

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