using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ElectronicsStore.Admin.ViewComponents
{
    public class LowStockAlertViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;

        public LowStockAlertViewComponent(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Tìm 5 sản phẩm sắp hết hàng (số lượng < 5)
            var lowStockProducts = await _context.SanPhams
                .Where(p => p.SoLuongTonKho < 5 && p.TrangThai)
                .OrderBy(p => p.SoLuongTonKho)
                .Take(5)
                .ToListAsync();

            return View("Default", lowStockProducts);
        }
    }
}
