using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ElectronicsStore.Customer.ViewComponents
{
    public class TopBrandsViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;

        public TopBrandsViewComponent(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy 6 thương hiệu phổ biến nhất
            var brands = await _context.NhaSanXuats
                .Where(b => b.TrangThai)
                .OrderBy(b => b.MaNhaSX)
                .Take(6)
                .ToListAsync();

            return View("Default", brands);
        }
    }
}
