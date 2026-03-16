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
                    ViewBag.ErrorCategory = "Lỗi API Danh mục: " + ex.Message; 
                }
                ViewBag.Categories = categories;

                // B. LẤY TẤT CẢ SẢN PHẨM TỪ API
                var allProducts = new List<ProductViewModel>();
                try 
                {
                    allProducts = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products") ?? new List<ProductViewModel>();
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorProduct = "Lỗi API Sản phẩm: " + ex.Message;
                }

                // C. TẠO MEGA MENU TỪ "TẤT CẢ SẢN PHẨM" (Để menu cột trái luôn hiện đủ danh sách hãng)
                var categoryBrands = allProducts
                    .Where(p => p.nhaSanXuat != null)
                    .GroupBy(p => p.maDanhMuc)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(p => p.nhaSanXuat!).DistinctBy(b => b.maNhaSX).ToList()
                    );
                
                ViewBag.CategoryBrands = categoryBrands;

                // D. XỬ LÝ LỌC SẢN PHẨM THEO QUERY URL
                var filteredProducts = allProducts.AsEnumerable();

                if (categoryId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.maDanhMuc == categoryId.Value);
                }

                if (brandId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.nhaSanXuat != null && p.nhaSanXuat.maNhaSX == brandId.Value);
                }

                // Cập nhật FilterTitle cho giao diện hiển thị xịn xò
                if (categoryId.HasValue || brandId.HasValue)
                {
                    var catName = categories.FirstOrDefault(c => c.maDanhMuc == categoryId.Value)?.tenDanhMuc ?? "";
                    var brandName = allProducts.FirstOrDefault(p => p.nhaSanXuat != null && p.nhaSanXuat.maNhaSX == brandId)?.nhaSanXuat?.tenNhaSX ?? "";
                    
                    string title = "Sản phẩm lọc theo: ";
                    if (!string.IsNullOrEmpty(catName)) title += $"[{catName}] ";
                    if (!string.IsNullOrEmpty(brandName)) title += $"Hãng {brandName}";
                    ViewBag.FilterTitle = title.Trim();
                }
                else
                {
                    ViewBag.FilterTitle = "Tất cả sản phẩm"; 
                }
                
                // Trả về danh sách ĐÃ ĐƯỢC LỌC
                return View(filteredProducts.ToList());
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