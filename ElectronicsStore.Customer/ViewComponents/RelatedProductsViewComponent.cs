using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ElectronicsStore.Customer.ViewComponents
{
    public class RelatedProductsViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;

        public RelatedProductsViewComponent(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int currentProductId)
        {
            // Tìm 4 sản phẩm cùng danh mục, trừ sản phẩm hiện tại ra, ưu tiên lượt xem cao
            var relatedProducts = await _context.SanPhams
                .Where(p => p.TrangThai && p.MaDanhMuc == categoryId && p.MaSP != currentProductId)
                .OrderByDescending(p => p.SoLuotXem)
                .Take(4)
                .ToListAsync();

            return View("Default", relatedProducts);
        }
    }
}
