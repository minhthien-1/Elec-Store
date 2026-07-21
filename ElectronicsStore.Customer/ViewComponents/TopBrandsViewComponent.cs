using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ElectronicsStore.API.Models.Entities;

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

            // Nếu DB chưa có data (do chưa chạy seed), tự động tạo mock data để giao diện không bị trống
            if (!brands.Any())
            {
                brands = new List<NhaSanXuat>
                {
                    new NhaSanXuat { MaNhaSX = 1, TenNhaSX = "Apple" },
                    new NhaSanXuat { MaNhaSX = 2, TenNhaSX = "Samsung" },
                    new NhaSanXuat { MaNhaSX = 3, TenNhaSX = "Sony" },
                    new NhaSanXuat { MaNhaSX = 4, TenNhaSX = "Dell" },
                    new NhaSanXuat { MaNhaSX = 5, TenNhaSX = "Asus" },
                    new NhaSanXuat { MaNhaSX = 6, TenNhaSX = "LG" }
                };
            }

            return View("Default", brands);
        }
    }
}
