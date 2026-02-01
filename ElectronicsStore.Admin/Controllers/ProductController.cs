using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ElectronicsStore.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProductController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        // Kiểm tra authentication
        private bool CheckAuth()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        // GET: Danh sách sản phẩm
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // TẠM THỜI BỎ QUA KIỂM TRA ĐĂNG NHẬP ĐỂ TEST GIAO DIỆN
            // Uncomment dòng này khi đã có API:
            // if (!CheckAuth())
            // {
            //     return RedirectToAction("Index", "Login");
            // }

            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/product";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<Product>>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(products);
                }
                else
                {
                    // NẾU API CHƯA CÓ, TẠO DỮ LIỆU MẪU ĐỂ TEST
                    var sampleProducts = new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            Name = "iPhone 15 Pro Max",
                            Price = 29990000,
                            Description = "Điện thoại cao cấp với chip A17 Pro",
                            Quantity = 50,
                            Category = "Điện thoại",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-thumbnew-600x600.jpg"
                        },
                        new Product
                        {
                            Id = 2,
                            Name = "Samsung Galaxy S24 Ultra",
                            Price = 26990000,
                            Description = "Flagship Samsung với S Pen",
                            Quantity = 30,
                            Category = "Điện thoại",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/320721/samsung-galaxy-s24-ultra-grey-thumbnew-600x600.jpg"
                        },
                        new Product
                        {
                            Id = 3,
                            Name = "MacBook Pro 14 M3",
                            Price = 45990000,
                            Description = "Laptop chuyên nghiệp cho developer",
                            Quantity = 15,
                            Category = "Laptop",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/309016/macbook-pro-14-inch-m3-2023-xam-1.jpg"
                        },
                        new Product
                        {
                            Id = 4,
                            Name = "iPad Pro M2 11 inch",
                            Price = 22990000,
                            Description = "Tablet mạnh mẽ cho công việc sáng tạo",
                            Quantity = 25,
                            Category = "Tablet",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/522/247509/ipad-pro-11-2022-wifi-gray-thumb-600x600.jpg"
                        },
                        new Product
                        {
                            Id = 5,
                            Name = "AirPods Pro 2",
                            Price = 5990000,
                            Description = "Tai nghe chống ồn cao cấp",
                            Quantity = 100,
                            Category = "Tai nghe",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/54/289780/tai-nghe-bluetooth-airpods-pro-2-apple-usb-c-1.jpg"
                        },
                        new Product
                        {
                            Id = 6,
                            Name = "Apple Watch Series 9",
                            Price = 10990000,
                            Description = "Đồng hồ thông minh từ Apple",
                            Quantity = 40,
                            Category = "Đồng hồ thông minh",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/7077/309020/apple-watch-s9-lte-41mm-thumb-pink-1-600x600.jpg"
                        },
                        new Product
                        {
                            Id = 7,
                            Name = "Sony WH-1000XM5",
                            Price = 8990000,
                            Description = "Tai nghe chụp tai chống ồn tốt nhất",
                            Quantity = 2,
                            Category = "Tai nghe",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/54/289546/tai-nghe-bluetooth-sony-wh-1000xm5-den-thumb-600x600.jpg"
                        },
                        new Product
                        {
                            Id = 8,
                            Name = "Dell XPS 13",
                            Price = 32990000,
                            Description = "Laptop mỏng nhẹ cao cấp",
                            Quantity = 0,
                            Category = "Laptop",
                            ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/236037/dell-xps-13-9320-i7-71003200-600x600.jpg"
                        }
                    };

                    ViewBag.ErrorMessage = "Đang sử dụng dữ liệu mẫu (API chưa kết nối)";
                    return View(sampleProducts);
                }
            }
            catch (Exception ex)
            {
                // NẾU CÓ LỖI, DÙNG DỮ LIỆU MẪU
                var sampleProducts = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "iPhone 15 Pro Max",
                        Price = 29990000,
                        Description = "Điện thoại cao cấp với chip A17 Pro",
                        Quantity = 50,
                        Category = "Điện thoại",
                        ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-thumbnew-600x600.jpg"
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Samsung Galaxy S24 Ultra",
                        Price = 26990000,
                        Description = "Flagship Samsung với S Pen",
                        Quantity = 30,
                        Category = "Điện thoại",
                        ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/320721/samsung-galaxy-s24-ultra-grey-thumbnew-600x600.jpg"
                    }
                };

                ViewBag.ErrorMessage = "Đang sử dụng dữ liệu mẫu (Lỗi: " + ex.Message + ")";
                return View(sampleProducts);
            }
        }

        // GET: Trang thêm sản phẩm
        [HttpGet]
        public IActionResult Create()
        {
            // Tạm thời bỏ qua check auth để test
            // if (!CheckAuth())
            // {
            //     return RedirectToAction("Index", "Login");
            // }
            return View();
        }

        // POST: Thêm sản phẩm mới
        [HttpPost]
        public IActionResult Create(Product product)
        {
            // Tạm thời chỉ hiển thị thông báo, không gọi API
            TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // GET: Trang sửa sản phẩm
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Tạm thời bỏ qua check auth để test
            // if (!CheckAuth())
            // {
            //     return RedirectToAction("Index", "Login");
            // }

            // Lấy dữ liệu từ session của Index
            var sessionProducts = HttpContext.Session.GetString("Products");
            if (!string.IsNullOrEmpty(sessionProducts))
            {
                var products = JsonSerializer.Deserialize<List<Product>>(sessionProducts, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var product = products?.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    return View(product);
                }
            }

            // Dữ liệu mẫu để test khi không có session
            var sampleProduct = new Product
            {
                Id = id,
                Name = "iPhone 15 Pro Max",
                Price = 29990000,
                Description = "Điện thoại cao cấp",
                Quantity = 50,
                Category = "Điện thoại",
                ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-thumbnew-600x600.jpg"
            };
            return View(sampleProduct);
        }

        // POST: Cập nhật sản phẩm
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            // Tạm thời chỉ hiển thị thông báo, không gọi API
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // POST: Xóa sản phẩm
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Tạm thời chỉ hiển thị thông báo, không gọi API
            return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
        }
    }

    // Model sản phẩm
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
    }
}