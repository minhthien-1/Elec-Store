using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using System.Security.Claims;

namespace ElectronicsStore.Customer.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được dùng giỏ hàng
    public class CartController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public CartController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // 1. Xem giỏ hàng (GET: /Cart)
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var cartItems = await _context.GioHangs
                .Include(g => g.SanPham) // Lấy thông tin sản phẩm đi kèm
                .Where(g => g.MaND == userId)
                .OrderByDescending(g => g.NgayThem)
                .ToListAsync();

            return View(cartItems);
        }

        // 2. Thêm vào giỏ (POST: /Cart/AddToCart)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            // Kiểm tra tồn kho (Optional - logic nâng cao)
            var product = await _context.SanPhams.FindAsync(productId);
            if (product == null || product.SoLuongTonKho < quantity)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng hoặc không đủ số lượng!";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartItem = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaND == userId && g.MaSP == productId);

            if (cartItem != null)
            {
                cartItem.SoLuong += quantity;
                _context.GioHangs.Update(cartItem);
            }
            else
            {
                var newCartItem = new GioHang
                {
                    MaND = userId,
                    MaSP = productId,
                    SoLuong = quantity,
                    NgayThem = DateTime.UtcNow // Fix lỗi ngày tháng PostgreSQL
                };
                _context.GioHangs.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm vào giỏ hàng!";
            return RedirectToAction("Index"); // Chuyển hướng về trang giỏ hàng
        }

        // 3. Xóa khỏi giỏ (POST: /Cart/Remove)
        [HttpPost]

        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var cartItem = await _context.GioHangs.Include(g => g.SanPham)
                .FirstOrDefaultAsync(g => g.MaND == userId && g.MaSP == productId);

            if (cartItem != null)
            {
                cartItem.SoLuong = quantity;
                if (cartItem.SoLuong <= 0) _context.GioHangs.Remove(cartItem); // Xóa nếu sl <= 0
                else _context.GioHangs.Update(cartItem);

                await _context.SaveChangesAsync();

                // Tính lại tổng tiền giỏ hàng để trả về cho Client
                var totalCart = await _context.GioHangs
                    .Where(g => g.MaND == userId)
                    .SumAsync(x => x.SoLuong * x.SanPham.GiaBan);

                return Ok(new { success = true, newTotal = totalCart });
            }
            return BadRequest();
        }
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            var cartItem = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaND == userId && g.MaSP == productId);

            if (cartItem != null)
            {
                _context.GioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ.";
            }
            return RedirectToAction("Index");
        }

        // Helper lấy UserID
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}

