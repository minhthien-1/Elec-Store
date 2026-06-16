using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Customer.ViewComponents
{
    /// <summary>
    /// View Component - Render menu danh mục sản phẩm
    /// Tự động fetch dữ liệu từ DB mà không cần Controller mớm dữ liệu
    /// </summary>
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<CategoryMenuViewComponent> _logger;

        public CategoryMenuViewComponent(ElectronicsStoreDbContext context, ILogger<CategoryMenuViewComponent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                _logger.LogInformation("[CategoryMenu] Bắt đầu fetch danh mục từ DB...");

                // 1. Lấy tất cả danh mục HOẠT ĐỘNG (TrangThai = true)
                var categories = await _context.DanhMucSanPhams
                    .Where(c => c.TrangThai)
                    .OrderBy(c => c.TenDanhMuc)
                    .ToListAsync();

                _logger.LogInformation($"[CategoryMenu] Lấy được {categories.Count} danh mục");

                return View("Default", categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CategoryMenu ERROR] {ex.Message}");
                // Trả về danh sách rỗng nếu có lỗi
                return View("Default", new List<dynamic>());
            }
        }
    }
}
