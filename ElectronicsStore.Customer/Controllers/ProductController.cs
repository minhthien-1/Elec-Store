using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.Customer.Models.ViewModels;
using ElectronicsStore.API.Models.Entities;
using System.Security.Claims;

namespace ElectronicsStore.Customer.Controllers
{
    public class ProductController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public ProductController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return RedirectToAction("Index", "Home");

            // 1. Lấy sản phẩm chính
            var productEntity = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.NhaSanXuat)
                .Include(p => p.DanhGias).ThenInclude(dg => dg.NguoiDung)
                .FirstOrDefaultAsync(p => p.MaSP == id && p.TrangThai == true);

            if (productEntity == null) return NotFound();

            // 2. Tăng view
            productEntity.SoLuotXem += 1;
            _context.SanPhams.Update(productEntity);
            await _context.SaveChangesAsync();
            // 3. LẤY SẢN PHẨM TƯƠNG TỰ (Logic thông minh: Vơ vét hết các sp liên quan)
            var similarProducts = await _context.SanPhams
                .Where(p => p.TrangThai == true
                            && p.MaSP != id // Trừ chính nó ra
                                            // Đổi && thành || (HOẶC) để lấy được iPhone (cùng hãng) hoặc Dell (cùng loại)
                            && (p.MaNhaSX == productEntity.MaNhaSX || p.MaDanhMuc == productEntity.MaDanhMuc))
                .OrderByDescending(p => p.MaNhaSX == productEntity.MaNhaSX && p.MaDanhMuc == productEntity.MaDanhMuc) // Ưu tiên 1: Giống y chang
                .ThenByDescending(p => p.MaNhaSX == productEntity.MaNhaSX) // Ưu tiên 2: Cùng Hãng (Apple)
                .ThenByDescending(p => p.ThemTrongDB) // Ưu tiên 3: Mới nhất
                .Take(4)
                .Select(p => new ProductThumbnailDto
                {
                    MaSP = p.MaSP,
                    TenSP = p.TenSP,
                    HinhAnh = p.HinhAnh,
                    GiaBan = p.GiaBan
                })
                .ToListAsync();

            // 4. Map sang ViewModel
            var viewModel = new ProductDetailViewModel
            {
                MaSP = productEntity.MaSP,
                TenSP = productEntity.TenSP,
                MoTaChiTiet = productEntity.MoTaChiTiet,
                ThongTinKyThuat = productEntity.ThongTinKyThuat,
                GiaBan = productEntity.GiaBan,
                GiaGiamGia = productEntity.GiaGiamGia,
                SoLuongTonKho = productEntity.SoLuongTonKho,
                HinhAnh = productEntity.HinhAnh,
                DanhGiaXepHang = productEntity.DanhGiaXepHang,
                SoLuotDanhGia = productEntity.SoLuotDanhGia,
                SoLuotXem = productEntity.SoLuotXem,

                DanhMuc = new CategoryDto { MaDanhMuc = productEntity.DanhMuc?.MaDanhMuc ?? 0, TenDanhMuc = productEntity.DanhMuc?.TenDanhMuc ?? "N/A" },
                NhaSX = new BrandDto { MaNhaSX = productEntity.NhaSanXuat?.MaNhaSX ?? 0, TenNhaSX = productEntity.NhaSanXuat?.TenNhaSX ?? "N/A" },

                DanhGias = productEntity.DanhGias.Select(dg => new ReviewDto
                {
                    MaDG = dg.MaDG,
                    DemSao = dg.DemSao,
                    NoiDung = dg.NoiDung,
                    ThemTrongDB = dg.ThemTrongDB,
                    NguoiDung = new UserDto { TenDayDu = dg.NguoiDung?.TenDayDu ?? "Khách hàng" }
                }).OrderByDescending(d => d.ThemTrongDB).ToList(),

                // Gán danh sách tương tự vào
                SanPhamTuongTu = similarProducts
            };

            return View(viewModel);
        }

        // POST: Gửi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int MaSP, int DemSao, string NoiDung)
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá!";
                return RedirectToAction("Details", new { id = MaSP });
            }

            try
            {
                // Lấy UserId từ Claims
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Index", "Home");
                int userId = int.Parse(userIdClaim);

                var review = new DanhGiaSanPham
                {
                    MaSP = MaSP,
                    MaND = userId,
                    DemSao = DemSao,
                    NoiDung = NoiDung,
                    ThemTrongDB = DateTime.UtcNow,
                    DuocDuyet = true // Tự động duyệt (hoặc để false nếu cần Admin duyệt)
                };

                _context.DanhGiaSanPhams.Add(review);
                await _context.SaveChangesAsync();

                // Cập nhật lại điểm đánh giá trung bình cho sản phẩm (Logic phụ)
                var product = await _context.SanPhams.Include(p => p.DanhGias).FirstOrDefaultAsync(p => p.MaSP == MaSP);
                if (product != null && product.DanhGias.Any())
                {
                    product.SoLuotDanhGia = product.DanhGias.Count;
                    product.DanhGiaXepHang = (decimal)product.DanhGias.Average(d => d.DemSao);
                    _context.SanPhams.Update(product);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi gửi đánh giá: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = MaSP });
        }
    }
}