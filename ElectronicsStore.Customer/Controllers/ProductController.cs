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
                ViewBag.ErrorGeneral = "Loi he thong: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"products/{id}");
                if (product == null) return NotFound();

                // Lay danh sach voucher dang hoat dong de hien thi dropdown
                var vouchers = new List<VoucherViewModel>();
                try
                {
                    var voucherResponse = await _httpClient.GetFromJsonAsync<VoucherListResponse>("promotion");
                    if (voucherResponse?.data != null)
                        vouchers = voucherResponse.data;
                }
                catch { }
                ViewBag.Vouchers = vouchers;

                // DECORATOR PATTERN
                Console.WriteLine("\n========== DECORATOR PATTERN ==========");
                Console.WriteLine($"San pham: {product.tenSP} | Gia goc: {product.giaBan:N0} VND");

                var orderGiftWrap = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                        .WithGiftWrap().Build();
                Console.WriteLine(orderGiftWrap.GetDescription());
                Console.WriteLine($"=> Tong tien: {orderGiftWrap.GetTotalPrice():N0} VND\n");

                var orderGold = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                    .WithGiftWrap().WithMemberDiscount("Gold").Build();
                Console.WriteLine(orderGold.GetDescription());
                Console.WriteLine($"=> Tong tien: {orderGold.GetTotalPrice():N0} VND\n");

                var orderPlatinum = new OrderServiceBuilder(product.tenSP, product.giaBan)
                                        .WithMemberDiscount("Platinum").Build();
                Console.WriteLine(orderPlatinum.GetDescription());
                Console.WriteLine($"=> Tong tien: {orderPlatinum.GetTotalPrice():N0} VND");

                // ABSTRACT FACTORY PATTERN
                Console.WriteLine("\n========== ABSTRACT FACTORY PATTERN ==========");
                var customerFactory = StoreFactoryProvider.GetFactory("customer");
                var customerProduct = customerFactory.CreateElectronicProduct("phone");
                customerProduct.MaSP = product.maSP;
                customerProduct.TenSP = product.tenSP;
                customerProduct.GiaBan = product.giaBan;
                Console.WriteLine("[CUSTOMER FACTORY] " + customerProduct.GetDisplayInfo());

                var adminFactory = StoreFactoryProvider.GetFactory("admin");
                var adminProduct = adminFactory.CreateElectronicProduct("phone");
                adminProduct.MaSP = product.maSP;
                adminProduct.TenSP = product.tenSP;
                adminProduct.GiaBan = product.giaBan;
                Console.WriteLine("[ADMIN FACTORY]    " + adminProduct.GetDisplayInfo());
                Console.WriteLine("========================================\n");

                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi goi API Details: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // POST: Khach chon voucher tu dropdown -> tinh gia -> ghi Terminal
        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(int productId, string maCode, decimal giaGoc)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<VoucherValidateResponse>(
                    $"promotion/validate/{maCode}?tongGia={giaGoc}");

                if (response == null)
                    return Json(new { success = false, message = "Khong the validate voucher" });

                // DECORATOR PATTERN voi Voucher tu DB
                Console.WriteLine("\n========== DECORATOR + VOUCHER TU DB ==========");
                Console.WriteLine($"Khach chon voucher: [{maCode}]");

                var orderWithVoucher = new OrderServiceBuilder($"San pham #{productId}", giaGoc)
                                           .WithVoucher(maCode, response.kieuGiam, response.giaTriGiam, response.giaTriGiamToiDa)
                                           .Build();

                Console.WriteLine(orderWithVoucher.GetDescription());
                Console.WriteLine($"=> Tong tien sau voucher: {orderWithVoucher.GetTotalPrice():N0} VND");
                Console.WriteLine("================================================\n");

                return Json(new
                {
                    success = true,
                    message = response.message,
                    discount = response.discount,
                    finalPrice = response.finalPrice
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Voucher khong hop le: " + ex.Message });
            }
        }
    }

    public class VoucherViewModel
    {
        public int maKM { get; set; }
        public string maCode { get; set; } = string.Empty;
        public string? tenChienDich { get; set; }
        public string? kieuGiam { get; set; }
        public decimal? giaTriGiam { get; set; }
        public decimal? giaTriGiamToiDa { get; set; }
        public decimal? giaTriDonHangToiThieu { get; set; }
    }

    public class VoucherListResponse
    {
        public int total { get; set; }
        public List<VoucherViewModel> data { get; set; } = new();
    }

    public class VoucherValidateResponse
    {
        public bool valid { get; set; }
        public string maCode { get; set; } = string.Empty;
        public string kieuGiam { get; set; } = string.Empty;
        public decimal giaTriGiam { get; set; }
        public decimal? giaTriGiamToiDa { get; set; }
        public decimal discount { get; set; }
        public decimal finalPrice { get; set; }
        public string message { get; set; } = string.Empty;
    }
}