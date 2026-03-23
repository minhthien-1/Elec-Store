using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ElectronicsStoreDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /api/product
        [HttpGet]
        public async Task<ActionResult<object>> GetProducts(
            [FromQuery] int? maDanhMuc = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 12;
                if (pageSize > 100) pageSize = 100; // Max 100 items per page

                var query = _context.SanPhams
                    .Where(s => s.TrangThai == true)
                    .AsQueryable();

                // Filter by category
                if (maDanhMuc.HasValue && maDanhMuc > 0)
                {
                    query = query.Where(s => s.MaDanhMuc == maDanhMuc);
                }

                // Filter by price range
                if (minPrice.HasValue)
                {
                    query = query.Where(s => s.GiaGiamGia >= minPrice || (s.GiaGiamGia == null && s.GiaBan >= minPrice));
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(s => s.GiaGiamGia <= maxPrice || (s.GiaGiamGia == null && s.GiaBan <= maxPrice));
                }

                // Search by name or description
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    query = query.Where(s => s.TenSP.Contains(searchKeyword) || s.MoTaChiTiet.Contains(searchKeyword));
                }

                var total = await query.CountAsync();

                var products = await query
                    .Include(s => s.DanhMuc)
                    .Include(s => s.NhaSanXuat)
                    .OrderByDescending(s => s.ThemTrongDB)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.MaSP,
                        s.TenSP,
                        s.MoTaChiTiet,
                        s.GiaBan,
                        s.GiaGiamGia,
                        s.SoLuongTonKho,
                        s.HinhAnh,
                        s.DanhGiaXepHang,
                        s.SoLuotDanhGia,
                        DanhMuc = new { s.DanhMuc!.MaDanhMuc, s.DanhMuc.TenDanhMuc },
                        NhaSX = new { s.NhaSanXuat!.MaNhaSX, s.NhaSanXuat.TenNhaSX }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total,
                    pageSize,
                    page,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProducts: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProductById(int id)
        {
            try
            {
                var product = await _context.SanPhams
                    .Where(s => s.MaSP == id && s.TrangThai == true)
                    .Include(s => s.DanhMuc)
                    .Include(s => s.NhaSanXuat)
                    .Include(s => s.DanhGias.Where(dg => dg.DuocDuyet == true))
                    .FirstOrDefaultAsync();

                if (product == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                // Increment view count
                product.SoLuotXem += 1;
                _context.SanPhams.Update(product);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    maSP = product.MaSP,
                    tenSP = product.TenSP,
                    moTaChiTiet = product.MoTaChiTiet,
                    thongTinKyThuat = product.ThongTinKyThuat,
                    giaBan = product.GiaBan,
                    giaGiamGia = product.GiaGiamGia,
                    soLuongTonKho = product.SoLuongTonKho,
                    hinhAnh = product.HinhAnh,
                    danhGiaXepHang = product.DanhGiaXepHang,
                    soLuotDanhGia = product.SoLuotDanhGia,
                    soLuotXem = product.SoLuotXem,
                    danhMuc = new { product.DanhMuc!.MaDanhMuc, product.DanhMuc.TenDanhMuc },
                    nhaSX = new { product.NhaSanXuat!.MaNhaSX, product.NhaSanXuat.TenNhaSX },
                    danhGias = product.DanhGias.Select(dg => new
                    {
                        dg.MaDG,
                        dg.DemSao,
                        dg.TieuDe,
                        dg.NoiDung,
                        dg.SoLuotThichChu,
                        dg.ThemTrongDB,
                        NguoiDung = new { dg.NguoiDung!.TenDayDu }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductById: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/product (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<object>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.TenSP))
                    return BadRequest(new { message = "Tên sản phẩm không được để trống" });

                if (request.GiaBan <= 0)
                    return BadRequest(new { message = "Giá bán phải lớn hơn 0" });

                if (request.MaDanhMuc <= 0)
                    return BadRequest(new { message = "Mã danh mục không hợp lệ" });

                // Check if category exists
                var danhMuc = await _context.DanhMucSanPhams.FindAsync(request.MaDanhMuc);
                if (danhMuc == null)
                    return NotFound(new { message = "Danh mục không tồn tại" });

                // Check if manufacturer exists (if provided)
                if (request.MaNhaSX.HasValue && request.MaNhaSX > 0)
                {
                    var nhaSX = await _context.NhaSanXuats.FindAsync(request.MaNhaSX);
                    if (nhaSX == null)
                        return NotFound(new { message = "Nhà sản xuất không tồn tại" });
                }

                var sanPham = new SanPham
                {
                    TenSP = request.TenSP,
                    MoTaChiTiet = request.MoTaChiTiet,
                    ThongTinKyThuat = request.ThongTinKyThuat,
                    MaDanhMuc = request.MaDanhMuc,
                    MaNhaSX = request.MaNhaSX,
                    GiaBan = request.GiaBan,
                    GiaGiamGia = request.GiaGiamGia,
                    SoLuongTonKho = request.SoLuongTonKho ?? 0,
                    HinhAnh = request.HinhAnh,
                    TrangThai = request.TrangThai,
                    ThemTrongDB = DateTime.Now
                };

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin created product: {sanPham.TenSP} (ID: {sanPham.MaSP})");

                return Created($"api/product/{sanPham.MaSP}", new
                {
                    message = "Tạo sản phẩm thành công",
                    product = new
                    {
                        sanPham.MaSP,
                        sanPham.TenSP,
                        sanPham.GiaBan,
                        sanPham.SoLuongTonKho
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateProduct: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/product/{id} (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(id);
                if (sanPham == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                // Validate input
                if (!string.IsNullOrWhiteSpace(request.TenSP))
                {
                    sanPham.TenSP = request.TenSP;
                }

                if (request.GiaBan.HasValue && request.GiaBan > 0)
                {
                    sanPham.GiaBan = request.GiaBan.Value;
                }

                if (request.GiaGiamGia.HasValue)
                {
                    if (request.GiaGiamGia < 0)
                        return BadRequest(new { message = "Giá giảm giá không hợp lệ" });
                    sanPham.GiaGiamGia = request.GiaGiamGia;
                }

                if (request.SoLuongTonKho.HasValue && request.SoLuongTonKho >= 0)
                {
                    sanPham.SoLuongTonKho = request.SoLuongTonKho.Value;
                }

                if (request.MaDanhMuc.HasValue && request.MaDanhMuc > 0)
                {
                    var danhMuc = await _context.DanhMucSanPhams.FindAsync(request.MaDanhMuc);
                    if (danhMuc == null)
                        return NotFound(new { message = "Danh mục không tồn tại" });
                    sanPham.MaDanhMuc = request.MaDanhMuc.Value;
                }

                if (request.MaNhaSX.HasValue && request.MaNhaSX > 0)
                {
                    var nhaSX = await _context.NhaSanXuats.FindAsync(request.MaNhaSX);
                    if (nhaSX == null)
                        return NotFound(new { message = "Nhà sản xuất không tồn tại" });
                    sanPham.MaNhaSX = request.MaNhaSX;
                }

                if (!string.IsNullOrWhiteSpace(request.MoTaChiTiet))
                {
                    sanPham.MoTaChiTiet = request.MoTaChiTiet;
                }

                if (!string.IsNullOrWhiteSpace(request.ThongTinKyThuat))
                {
                    sanPham.ThongTinKyThuat = request.ThongTinKyThuat;
                }

                if (!string.IsNullOrWhiteSpace(request.HinhAnh))
                {
                    sanPham.HinhAnh = request.HinhAnh;
                }

                sanPham.TrangThai = request.TrangThai;
                sanPham.SuaDoi = DateTime.Now;

                _context.SanPhams.Update(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin updated product: {sanPham.TenSP} (ID: {sanPham.MaSP})");

                return Ok(new
                {
                    message = "Cập nhật sản phẩm thành công",
                    product = new
                    {
                        sanPham.MaSP,
                        sanPham.TenSP,
                        sanPham.GiaBan,
                        sanPham.SoLuongTonKho,
                        sanPham.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateProduct: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/product/{id} (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteProduct(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(id);
                if (sanPham == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                // Check if product is in any active orders
                var inOrders = await _context.ChiTietDonHangs
                    .Include(c => c.DonHang)
                    .AnyAsync(c => c.MaSP == id && c.DonHang.TrangThaiDon != "Đã hủy");

                if (inOrders)
                    return BadRequest(new { message = "Không thể xóa sản phẩm vì nó đang có trong đơn hàng" });

                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin deleted product: {sanPham.TenSP} (ID: {sanPham.MaSP})");

                return Ok(new { message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteProduct: {ex.Message}");
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        #region Request/Response Classes

        public class CreateProductRequest
        {
            [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
            [StringLength(255, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3-255 ký tự")]
            public string TenSP { get; set; } = string.Empty;

            public string? MoTaChiTiet { get; set; }
            public string? ThongTinKyThuat { get; set; }

            [Required(ErrorMessage = "Mã danh mục không được để trống")]
            [Range(1, int.MaxValue, ErrorMessage = "Mã danh mục không hợp lệ")]
            public int MaDanhMuc { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Mã nhà sản xuất không hợp lệ")]
            public int? MaNhaSX { get; set; }

            [Required(ErrorMessage = "Giá bán không được để trống")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
            public decimal GiaBan { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Giá giảm giá không hợp lệ")]
            public decimal? GiaGiamGia { get; set; }

            [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
            public int? SoLuongTonKho { get; set; }

            public string? HinhAnh { get; set; }
            public bool TrangThai { get; set; } = true;
        }

        public class UpdateProductRequest
        {
            [StringLength(255, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3-255 ký tự")]
            public string? TenSP { get; set; }

            public string? MoTaChiTiet { get; set; }
            public string? ThongTinKyThuat { get; set; }

            [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
            public decimal? GiaBan { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Giá giảm giá không hợp lệ")]
            public decimal? GiaGiamGia { get; set; }

            [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
            public int? SoLuongTonKho { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Mã danh mục không hợp lệ")]
            public int? MaDanhMuc { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Mã nhà sản xuất không hợp lệ")]
            public int? MaNhaSX { get; set; }

            public string? HinhAnh { get; set; }
            public bool TrangThai { get; set; } = true;
        }

        #endregion
    }
}
