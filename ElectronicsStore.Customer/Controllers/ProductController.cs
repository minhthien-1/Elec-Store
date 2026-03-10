using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models; // Dùng Model bạn mới tạo
using System.Net.Http.Json;

namespace ElectronicsStore.Customer.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController()
        {
            _httpClient = new HttpClient();
            // CHÚ Ý: Đổi số 7001 thành Port API của bạn
            _httpClient.BaseAddress = new Uri("http://localhost:5145/api/"); 
        }

        // 1. ACTION INDEX: Lấy danh sách từ API
        public async Task<IActionResult> Index(int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            try
            {
                // Gọi API lấy sản phẩm (Hiện tại API của bạn mới chỉ có GetAll cơ bản)
                // Sau này bạn có thể truyền thêm query string: products?categoryId=1...
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products");

                // Giữ lại logic hiển thị tiêu đề cho View
                ViewBag.FilterTitle = "Tất cả sản phẩm"; 
                
                return View(products);
            }
            catch
            {
                return View(new List<ProductViewModel>());
            }
        }

        // 2. ACTION DETAILS: Lấy chi tiết từ API
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Kiểm tra xem port 5145 có đúng là port API đang chạy không
                var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"products/{id}");
                
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                // In lỗi ra Console để Thien dễ debug nếu nó nhảy vào đây
                Console.WriteLine("Lỗi gọi API: " + ex.Message);
                return RedirectToAction("Index");
            }
            
        }
    }
}