using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;

        public CategoryController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /api/category
        [HttpGet]
        public async Task<ActionResult<object>> GetAllCategories()
        {
            try
            {
                var categories = await _context.DanhMucSanPhams
                    .Where(c => c.TrangThai == true)
                    .Select(c => new
                    {
                        c.MaDanhMuc,
                        c.TenDanhMuc,
                        c.MoTa,
                        c.HinhAnh,
                        SoSanPham = c.SanPhams.Count,
                        c.ThemTrongDB,
                        c.SuaDoi
                    })
                    .ToListAsync();

                return Ok(new { total = categories.Count, data = categories });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/category/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.DanhMucSanPhams
                    .Where(c => c.MaDanhMuc == id && c.TrangThai == true)
                    .Include(c => c.SanPhams.Where(s => s.TrangThai == true))
                    .FirstOrDefaultAsync();

                if (category == null)
                    return NotFound(new { message = "Danh mục không tồn tại" });

                return Ok(new
                {
                    maDanhMuc = category.MaDanhMuc,
                    tenDanhMuc = category.TenDanhMuc,
                    moTa = category.MoTa,
                    hinhAnh = category.HinhAnh,
                    sanPhams = category.SanPhams.Select(s => new
                    {
                        s.MaSP,
                        s.TenSP,
                        s.GiaBan,
                        s.GiaGiamGia,
                        s.HinhAnh,
                        s.DanhGiaXepHang,
                        s.SoLuotDanhGia
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/category
        [HttpPost]
        public async Task<ActionResult<object>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenDanhMuc))
                    return BadRequest(new { message = "Tên danh mục không được để trống" });

                var category = new DanhMucSanPham
                {
                    TenDanhMuc = request.TenDanhMuc,
                    MoTa = request.MoTa,
                    HinhAnh = request.HinhAnh,
                    TrangThai = request.TrangThai,
                    ThemTrongDB = DateTime.Now
                };

                _context.DanhMucSanPhams.Add(category);
                await _context.SaveChangesAsync();

                return Created($"api/category/{category.MaDanhMuc}", new
                {
                    message = "Tạo danh mục thành công",
                    category = new
                    {
                        category.MaDanhMuc,
                        category.TenDanhMuc,
                        category.MoTa,
                        category.HinhAnh,
                        category.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/category/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.DanhMucSanPhams.FindAsync(id);
                if (category == null)
                    return NotFound(new { message = "Danh mục không tồn tại" });

                if (!string.IsNullOrWhiteSpace(request.TenDanhMuc))
                    category.TenDanhMuc = request.TenDanhMuc;

                if (request.MoTa != null)
                    category.MoTa = request.MoTa;

                if (request.HinhAnh != null)
                    category.HinhAnh = request.HinhAnh;

                category.TrangThai = request.TrangThai;
                category.SuaDoi = DateTime.Now;

                _context.DanhMucSanPhams.Update(category);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật danh mục thành công",
                    category = new
                    {
                        category.MaDanhMuc,
                        category.TenDanhMuc,
                        category.MoTa,
                        category.HinhAnh,
                        category.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/category/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.DanhMucSanPhams.FindAsync(id);
                if (category == null)
                    return NotFound(new { message = "Danh mục không tồn tại" });

                // Check if category has products
                var hasProducts = await _context.SanPhams
                    .AnyAsync(s => s.MaDanhMuc == id);

                if (hasProducts)
                    return BadRequest(new { message = "Không thể xóa danh mục vì có sản phẩm đang sử dụng" });

                _context.DanhMucSanPhams.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class CreateCategoryRequest
        {
            public string TenDanhMuc { get; set; } = string.Empty;
            public string? MoTa { get; set; }
            public string? HinhAnh { get; set; }
            public bool TrangThai { get; set; } = true;
        }

        public class UpdateCategoryRequest
        {
            public string? TenDanhMuc { get; set; }
            public string? MoTa { get; set; }
            public string? HinhAnh { get; set; }
            public bool TrangThai { get; set; } = true;
        }
    }
}