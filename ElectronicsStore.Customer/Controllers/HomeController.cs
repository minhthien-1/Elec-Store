using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Services; 

namespace ElectronicsStore.Customer.Controllers
{
    public class HomeController : Controller
    {
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
                var products = await _productApiService.GetAllProductsAsync() ?? new List<ProductViewModel>();

                // 2. Lấy danh mục để hiển thị Sidebar 
                var categories = await _productApiService.GetCategoriesAsync();
                ViewBag.Categories = categories;

                // 3. BỔ SUNG LOGIC MEGA MENU (Gom nhóm thương hiệu theo danh mục)
                var categoryBrands = products
                    .Where(p => p.nhaSanXuat != null) // Nhớ đảm bảo ProductViewModel của bạn có thuộc tính nhaSanXuat nhé
                    .GroupBy(p => p.maDanhMuc)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(p => p.nhaSanXuat!).DistinctBy(b => b.maNhaSX).ToList()
                    );
                
                // Đẩy ra View để vòng lặp categoryBrands không bị rỗng
                ViewBag.CategoryBrands = categoryBrands;

                return View(products);
            }
            catch (Exception ex)
            {
                // Log lỗi ra console để debug
                Console.WriteLine($"Lỗi API tại HomeController: {ex.Message}");
                ViewBag.Categories = new List<CategoryViewModel>();
                ViewBag.CategoryBrands = new Dictionary<int, List<BrandViewModel>>();
                return View(new List<ProductViewModel>());
            }
        }
    }
}