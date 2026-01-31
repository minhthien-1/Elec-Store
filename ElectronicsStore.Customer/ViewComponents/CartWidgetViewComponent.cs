using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using System.Security.Claims;

namespace ElectronicsStore.Customer.ViewComponents
{
    public class CartWidgetViewComponent : ViewComponent
    {
        private readonly ElectronicsStoreDbContext _context;

        public CartWidgetViewComponent(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = ((ClaimsIdentity)User.Identity).FindFirst("UserId");
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim.Value);
                    count = await _context.GioHangs.Where(g => g.MaND == userId).SumAsync(g => g.SoLuong);
                }
            }
            return View(count);
        }
    }
}