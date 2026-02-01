using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Customer.Controllers
{
    public class OrderController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public OrderController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // Trang lịch sử đơn hàng & thanh toán
        public async Task<IActionResult> History()
        {
            // Kiểm tra đăng nhập
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            // Lấy danh sách đơn hàng kèm theo chi tiết sản phẩm và các lịch sử
            var orders = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham) // Lấy sản phẩm
                .Include(d => d.LichSuDonHangs)   // Lấy lịch sử trạng thái
                .Include(d => d.LichSuThanhToans) // Lấy lịch sử thanh toán
                .Where(d => d.MaND == userId)
                .OrderByDescending(d => d.NgayTaoDon) // Đơn mới nhất lên đầu
                .ToListAsync();

            return View(orders);
        }
    }
}