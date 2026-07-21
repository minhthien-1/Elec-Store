using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ElectronicsStore.Admin.ViewComponents
{
    public class PendingOrdersWidgetViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;

        public PendingOrdersWidgetViewComponent(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Đếm số lượng đơn hàng "Chờ xác nhận"
            int pendingCount = await _context.DonHangs
                .Where(d => d.TrangThaiDon == "Chờ xác nhận")
                .CountAsync();
                
            return View("Default", pendingCount);
        }
    }
}
