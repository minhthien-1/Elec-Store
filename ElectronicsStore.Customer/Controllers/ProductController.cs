using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Customer.Models;
using ElectronicsStore.Customer.Decorator;
using ElectronicsStore.AbstractFactory;
using ElectronicsStore.AbstractFactory.Factories;
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
                var categories = new List<CategoryViewModel>();
                try
                {
                    var catResponse = await _httpClient.GetFromJsonAsync<CategoryApiResponse>("category");
                    if (catResponse != null && catResponse.data != null)
                        categories = catResponse.data;
                }
                catch (Exception ex) { ViewBag.ErrorCategory = "Lỗi API Danh mục: " + ex.Message; }
                ViewBag.Categories = categories;

                var products = new List<ProductViewModel>();
                try
                {
                    products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products") ?? new List<ProductViewModel>();
                }
                catch (Exception ex) { ViewBag.ErrorProduct = "Lỗi API Sản phẩm: " + ex.Message; }

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

                // ══════════════════════════════════════════════════════
                // DECORATOR PATTERN
                // Mục đích: Cộng dồn dịch vụ vào đơn hàng (gói quà,
                // giảm giá) mà không sửa class gốc
                // ══════════════════════════════════════════════════════
                Console.WriteLine("\n========== DECORATOR PATTERN ==========");
                Console.WriteLine($"Sản phẩm: {product.tenSP} | Giá gốc: {product.giaBan:N0} VNĐ");

                // Chỉ gói quà
                var orderGiftWrap = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                        .WithGiftWrap()
                                        .Build();
                Console.WriteLine(orderGiftWrap.GetDescription());
                Console.WriteLine($"=> Tổng tiền: {orderGiftWrap.GetTotalPrice():N0} VNĐ\n");

                // Gói quà + Giảm giá Gold
                var orderGold = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                    .WithGiftWrap()
                                    .WithMemberDiscount("Gold")
                                    .Build();
                Console.WriteLine(orderGold.GetDescription());
                Console.WriteLine($"=> Tổng tiền: {orderGold.GetTotalPrice():N0} VNĐ\n");

                // Chỉ Giảm giá Platinum
                var orderPlatinum = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                        .WithMemberDiscount("Platinum")
                                        .Build();
                Console.WriteLine(orderPlatinum.GetDescription());
                Console.WriteLine($"=> Tổng tiền: {orderPlatinum.GetTotalPrice():N0} VNĐ");

                // ══════════════════════════════════════════════════════
                // ABSTRACT FACTORY PATTERN
                // Mục đích: Cùng 1 sản phẩm nhưng CustomerFactory và
                // AdminFactory tạo ra object với thông tin khác nhau.
                // Customer thấy thông tin đẹp để mua hàng,
                // Admin thấy thông tin quản lý để nhập kho.
                // ══════════════════════════════════════════════════════
                Console.WriteLine("\n========== ABSTRACT FACTORY PATTERN ==========");
                Console.WriteLine($"Cùng sản phẩm MaSP={id}, 2 factory tạo ra object khác nhau:\n");

                // Customer Factory: tạo object hiển thị cho khách mua hàng
                var customerFactory = StoreFactoryProvider.GetFactory("customer");
                var customerProduct = customerFactory.CreateElectronicProduct("phone");
                customerProduct.MaSP = product.maSP;
                customerProduct.TenSP = product.tenSP;
                customerProduct.GiaBan = product.giaBan;
                Console.WriteLine("[CUSTOMER FACTORY] Khách hàng thấy:");
                Console.WriteLine(customerProduct.GetDisplayInfo());

                Console.WriteLine();

                // Admin Factory: tạo object dùng cho quản trị
                var adminFactory = StoreFactoryProvider.GetFactory("admin");
                var adminProduct = adminFactory.CreateElectronicProduct("phone");
                adminProduct.MaSP = product.maSP;
                adminProduct.TenSP = product.tenSP;
                adminProduct.GiaBan = product.giaBan;
                Console.WriteLine("[ADMIN FACTORY] Admin thấy:");
                Console.WriteLine(adminProduct.GetDisplayInfo());

                Console.WriteLine("========================================\n");
                // ══════════════════════════════════════════════════════

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