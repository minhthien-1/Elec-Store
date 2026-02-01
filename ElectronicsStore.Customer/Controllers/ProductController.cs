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

        // ---------------------------------------------------------
        // 1. ACTION INDEX: HIỂN THỊ DANH SÁCH & LỌC SẢN PHẨM
        // ---------------------------------------------------------
        public async Task<IActionResult> Index(int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            // A. Chuẩn bị dữ liệu cho Sidebar / Mega Menu (Để menu không bị trống khi sang trang này)
            var categories = await _context.DanhMucSanPhams.Where(c => c.TrangThai == true).ToListAsync();

            // Lấy danh sách thương hiệu theo danh mục (để hiển thị trong Mega Menu)
            // Logic: Lấy tất cả sản phẩm -> Group theo danh mục -> Lấy list hãng sản xuất unique
            var allActiveProducts = await _context.SanPhams
                .Include(p => p.NhaSanXuat)
                .Where(p => p.TrangThai == true)
                .ToListAsync();

            var categoryBrands = allActiveProducts
                .Where(p => p.NhaSanXuat != null)
                .GroupBy(p => p.MaDanhMuc)
                .ToDictionary(g => g.Key, g => g.Select(p => p.NhaSanXuat).DistinctBy(b => b.MaNhaSX).ToList());

            ViewBag.Categories = categories;
            ViewBag.CategoryBrands = categoryBrands;

            // B. Truy vấn chính: Lọc sản phẩm
            var query = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.NhaSanXuat)
                .Where(p => p.TrangThai == true);

            // 1. Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.MaDanhMuc == categoryId.Value);
                // Gửi tên danh mục ra view để hiển thị tiêu đề (VD: "Laptop")
                var catName = categories.FirstOrDefault(c => c.MaDanhMuc == categoryId.Value)?.TenDanhMuc;
                ViewBag.FilterTitle = catName;
            }

            // 2. Lọc theo Thương hiệu
            if (brandId.HasValue)
            {
                query = query.Where(p => p.MaNhaSX == brandId.Value);
                // Nếu lọc cả danh mục và thương hiệu
                var brandName = _context.NhaSanXuats.FirstOrDefault(b => b.MaNhaSX == brandId.Value)?.TenNhaSX;
                ViewBag.FilterTitle = string.IsNullOrEmpty(ViewBag.FilterTitle) ? brandName : $"{ViewBag.FilterTitle} - {brandName}";
            }

            // 3. Lọc theo Giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.GiaBan >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.GiaBan <= maxPrice.Value);
            }

            // 4. Sắp xếp (Optional)
            switch (sortOrder)
            {
                case "price_desc":
                    query = query.OrderByDescending(p => p.GiaBan);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.GiaBan);
                    break;
                default:
                    query = query.OrderByDescending(p => p.ThemTrongDB); // Mặc định mới nhất
                    break;
            }

            var products = await query.ToListAsync();

            if (string.IsNullOrEmpty(ViewBag.FilterTitle)) ViewBag.FilterTitle = "Tất cả sản phẩm";

            return View(products);
        }

        // ---------------------------------------------------------
        // 2. ACTION DETAILS: CHI TIẾT SẢN PHẨM (Code cũ của bạn)
        // ---------------------------------------------------------
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

            // 3. LẤY SẢN PHẨM TƯƠNG TỰ
            var similarProducts = await _context.SanPhams
                .Where(p => p.TrangThai == true
                            && p.MaSP != id
                            && (p.MaNhaSX == productEntity.MaNhaSX || p.MaDanhMuc == productEntity.MaDanhMuc))
                .OrderByDescending(p => p.MaNhaSX == productEntity.MaNhaSX && p.MaDanhMuc == productEntity.MaDanhMuc)
                .ThenByDescending(p => p.MaNhaSX == productEntity.MaNhaSX)
                .ThenByDescending(p => p.ThemTrongDB)
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

                SanPhamTuongTu = similarProducts
            };

            return View(viewModel);
        }

        // ---------------------------------------------------------
        // 3. ACTION SUBMIT REVIEW: GỬI ĐÁNH GIÁ (Code cũ của bạn)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int MaSP, int DemSao, string NoiDung)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá!";
                return RedirectToAction("Details", new { id = MaSP });
            }

            try
            {
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
                    DuocDuyet = true
                };

                _context.DanhGiaSanPhams.Add(review);
                await _context.SaveChangesAsync();

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