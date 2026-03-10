using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Services; // Đảm bảo đúng Namespace của Service bạn đã viết

namespace ElectronicsStore.Customer.Controllers
{
    public class HomeController : Controller
    {
        // Sử dụng Service thay vì dùng HttpClient trực tiếp để code sạch hơn
        private readonly IProductApiService _productApiService;

        public HomeController(IProductApiService productApiService)
        {
            _productApiService = productApiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Lấy danh sách sản phẩm từ Service
                var products = await _productApiService.GetAllProductsAsync();

                // 2. Lấy danh mục để hiển thị Sidebar (Sửa lỗi Sidebar trắng)
                ViewBag.Categories = await _productApiService.GetCategoriesAsync();

                if (products == null || !products.Any())
                {
                    // Nếu không có sản phẩm, vẫn trả về View với danh sách trống thay vì dùng Content
                    return View(new List<ProductViewModel>());
                }

                return View(products);
            }
            catch (Exception ex)
            {
                // Log lỗi ra console để debug cho dễ
                Console.WriteLine($"Lỗi API: {ex.Message}");
                return View(new List<ProductViewModel>());
            }
        }
    }
}