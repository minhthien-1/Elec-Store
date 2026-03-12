using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models; 
using System.Net.Http.Json;

namespace ElectronicsStore.Customer.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5145/api/"); 
        }

    public async Task<IActionResult> Index(int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string sortOrder)
{
    try
    {
        // A. LẤY DANH MỤC
        var categories = new List<CategoryViewModel>();
        try 
        {
            var catResponse = await _httpClient.GetFromJsonAsync<CategoryApiResponse>("category");
            if (catResponse != null && catResponse.data != null)
            {
                categories = catResponse.data;
            }
        } 
        catch (Exception ex) 
        { 
            // GẮN LỖI VÀO VIEWBAG ĐỂ ĐẨY RA GIAO DIỆN
            ViewBag.ErrorCategory = "Lỗi API Danh mục: " + ex.Message; 
        }
        ViewBag.Categories = categories;

        // B. LẤY SẢN PHẨM
        var products = new List<ProductViewModel>();
        try 
        {
            products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products") ?? new List<ProductViewModel>();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorProduct = "Lỗi API Sản phẩm: " + ex.Message;
        }

        // C. TẠO MEGA MENU
        var categoryBrands = products
            .Where(p => p.nhaSanXuat != null)
            .GroupBy(p => p.maDanhMuc)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(p => p.nhaSanXuat!).DistinctBy(b => b.maNhaSX).ToList()
            );
        
        ViewBag.CategoryBrands = categoryBrands;
        ViewBag.FilterTitle = "Tất cả sản phẩm"; 
        
        return View(products);
    }
    catch (Exception ex)
    {
        ViewBag.ErrorGeneral = "Lỗi hệ thống: " + ex.Message;
        return View(new List<ProductViewModel>());
    }
}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"products/{id}");
                if (product == null) return NotFound();
                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gọi API Details: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}