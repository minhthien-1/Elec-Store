using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;

        public CartController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /api/cart/{maND}
        [HttpGet("{maND}")]
        public async Task<ActionResult<object>> GetCart(int maND)
        {
            try
            {
                var user = await _context.NguoiDungs.FindAsync(maND);
                if (user == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                var cartItems = await _context.GioHangs
                    .Where(g => g.MaND == maND)
                    .Include(g => g.SanPham)
                    .Select(g => new
                    {
                        g.MaGioHang,
                        g.MaSP,
                        g.SoLuong,
                        g.NgayThem,
                        SanPham = new
                        {
                            g.SanPham!.MaSP,
                            g.SanPham.TenSP,
                            g.SanPham.GiaBan,
                            g.SanPham.GiaGiamGia,
                            g.SanPham.SoLuongTonKho,
                            g.SanPham.HinhAnh
                        }
                    })
                    .ToListAsync();

                var tongTien = cartItems.Sum(item =>
                    (item.SanPham.GiaGiamGia ?? item.SanPham.GiaBan) * item.SoLuong
                );

                return Ok(new
                {
                    tongSanPham = cartItems.Count,
                    tongTien,
                    items = cartItems
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/cart/add
        [HttpPost("add")]
        public async Task<ActionResult<object>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (request.SoLuong <= 0)
                    return BadRequest(new { message = "Số lượng phải lớn hơn 0" });

                var user = await _context.NguoiDungs.FindAsync(request.MaND);
                if (user == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                var sanPham = await _context.SanPhams.FindAsync(request.MaSP);
                if (sanPham == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                if (sanPham.SoLuongTonKho < request.SoLuong)
                    return BadRequest(new { message = "Số lượng không đủ trong kho" });

                var existingItem = await _context.GioHangs
                    .FirstOrDefaultAsync(g => g.MaND == request.MaND && g.MaSP == request.MaSP);

                if (existingItem != null)
                {
                    existingItem.SoLuong += request.SoLuong;
                    _context.GioHangs.Update(existingItem);
                }
                else
                {
                    var gioHang = new GioHang
                    {
                        MaND = request.MaND,
                        MaSP = request.MaSP,
                        SoLuong = request.SoLuong,
                        NgayThem = DateTime.Now
                    };
                    _context.GioHangs.Add(gioHang);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Thêm vào giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/cart/update
        [HttpPut("update")]
        public async Task<ActionResult<object>> UpdateCart([FromBody] UpdateCartRequest request)
        {
            try
            {
                if (request.SoLuong <= 0)
                    return BadRequest(new { message = "Số lượng phải lớn hơn 0" });

                var cartItem = await _context.GioHangs.FindAsync(request.MaGioHang);
                if (cartItem == null)
                    return NotFound(new { message = "Mục giỏ hàng không tồn tại" });

                var sanPham = await _context.SanPhams.FindAsync(cartItem.MaSP);
                if (sanPham!.SoLuongTonKho < request.SoLuong)
                    return BadRequest(new { message = "Số lượng không đủ trong kho" });

                cartItem.SoLuong = request.SoLuong;
                _context.GioHangs.Update(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/cart/{maGioHang}
        [HttpDelete("{maGioHang}")]
        public async Task<ActionResult<object>> RemoveFromCart(int maGioHang)
        {
            try
            {
                var cartItem = await _context.GioHangs.FindAsync(maGioHang);
                if (cartItem == null)
                    return NotFound(new { message = "Mục giỏ hàng không tồn tại" });

                _context.GioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa khỏi giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/cart/clear/{maND}
        [HttpDelete("clear/{maND}")]
        public async Task<ActionResult<object>> ClearCart(int maND)
        {
            try
            {
                var cartItems = await _context.GioHangs
                    .Where(g => g.MaND == maND)
                    .ToListAsync();

                if (cartItems.Count == 0)
                    return NotFound(new { message = "Giỏ hàng trống" });

                _context.GioHangs.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa toàn bộ giỏ hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class AddToCartRequest
        {
            public int MaND { get; set; }
            public int MaSP { get; set; }
            public int SoLuong { get; set; }
        }

        public class UpdateCartRequest
        {
            public int MaGioHang { get; set; }
            public int SoLuong { get; set; }
        }
    }
}