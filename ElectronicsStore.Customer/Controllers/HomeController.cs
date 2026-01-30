using System.Diagnostics;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.Customer.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thiết để sử dụng .ToListAsync()

namespace ElectronicsStore.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ElectronicsStoreDbContext _context; // Khai báo DbContext để dùng trong Controller

        // Tiêm cả Logger và DbContext vào thông qua Constructor
        public HomeController(ILogger<HomeController> logger, ElectronicsStoreDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {


            try
            {
                // Kiểm tra xem Web có "chạm" vào được DB không
                bool canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return Content("Lỗi: Không thể kết nối tới server Supabase. Hãy kiểm tra Port 6543 hoặc Mật khẩu.");
                }

                var products = await _context.SanPhams.ToListAsync();
                // Lấy danh sách danh mục để làm menu bên trái
                var categories = await _context.DanhMucSanPhams
                                   .Where(c => c.TrangThai == true)
                                   .ToListAsync();
                ViewBag.Categories = categories;
                var categoryBrands = await _context.SanPhams
        .Include(p => p.NhaSanXuat)
        .Where(p => p.MaNhaSX != null)
        .GroupBy(p => p.MaDanhMuc)
        .Select(g => new {
            MaDanhMuc = g.Key,
            Brands = g.Select(p => p.NhaSanXuat).Distinct().ToList()
        })
        .ToDictionaryAsync(x => x.MaDanhMuc, x => x.Brands);

                ViewBag.CategoryBrands = categoryBrands;

                // Nếu DB có dữ liệu nhưng products rỗng, báo lỗi dữ liệu
                if (products == null || !products.Any())
                {
                    return Content("Kết nối thành công nhưng bảng SanPham không có dữ liệu!");
                }

                return View(products);
            }
            catch (Exception ex)
            {
                // Hiện lỗi thật sự lên trình duyệt để soi
                return Content("Lỗi kết nối cụ thể: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}