using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManufacturerController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;

        public ManufacturerController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /api/manufacturer
        [HttpGet]
        public async Task<ActionResult<object>> GetAllManufacturers()
        {
            try
            {
                var manufacturers = await _context.NhaSanXuats
                    .Where(m => m.TrangThai == true)
                    .Select(m => new
                    {
                        m.MaNhaSX,
                        m.TenNhaSX,
                        m.DatNuoc,
                        m.Email,
                        m.SoDienThoai,
                        m.Website,
                        SoSanPham = m.SanPhams.Count,
                        m.ThemTrongDB,
                        m.SuaDoi
                    })
                    .ToListAsync();

                return Ok(new { total = manufacturers.Count, data = manufacturers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/manufacturer/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetManufacturerById(int id)
        {
            try
            {
                var manufacturer = await _context.NhaSanXuats
                    .Where(m => m.MaNhaSX == id && m.TrangThai == true)
                    .Include(m => m.SanPhams.Where(s => s.TrangThai == true))
                    .FirstOrDefaultAsync();

                if (manufacturer == null)
                    return NotFound(new { message = "Nhà sản xuất không tồn tại" });

                return Ok(new
                {
                    maNhaSX = manufacturer.MaNhaSX,
                    tenNhaSX = manufacturer.TenNhaSX,
                    datNuoc = manufacturer.DatNuoc,
                    email = manufacturer.Email,
                    soDienThoai = manufacturer.SoDienThoai,
                    website = manufacturer.Website,
                    sanPhams = manufacturer.SanPhams.Select(s => new
                    {
                        s.MaSP,
                        s.TenSP,
                        s.GiaBan,
                        s.GiaGiamGia,
                        s.HinhAnh,
                        s.DanhGiaXepHang
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/manufacturer
        [HttpPost]
        public async Task<ActionResult<object>> CreateManufacturer([FromBody] CreateManufacturerRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenNhaSX))
                    return BadRequest(new { message = "Tên nhà sản xuất không được để trống" });

                var manufacturer = new NhaSanXuat
                {
                    TenNhaSX = request.TenNhaSX,
                    DatNuoc = request.DatNuoc,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    Website = request.Website,
                    TrangThai = request.TrangThai,
                    ThemTrongDB = DateTime.Now
                };

                _context.NhaSanXuats.Add(manufacturer);
                await _context.SaveChangesAsync();

                return Created($"api/manufacturer/{manufacturer.MaNhaSX}", new
                {
                    message = "Tạo nhà sản xuất thành công",
                    manufacturer = new
                    {
                        manufacturer.MaNhaSX,
                        manufacturer.TenNhaSX,
                        manufacturer.DatNuoc,
                        manufacturer.Email,
                        manufacturer.SoDienThoai,
                        manufacturer.Website,
                        manufacturer.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/manufacturer/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateManufacturer(int id, [FromBody] UpdateManufacturerRequest request)
        {
            try
            {
                var manufacturer = await _context.NhaSanXuats.FindAsync(id);
                if (manufacturer == null)
                    return NotFound(new { message = "Nhà sản xuất không tồn tại" });

                if (!string.IsNullOrWhiteSpace(request.TenNhaSX))
                    manufacturer.TenNhaSX = request.TenNhaSX;

                if (request.DatNuoc != null)
                    manufacturer.DatNuoc = request.DatNuoc;

                if (request.Email != null)
                    manufacturer.Email = request.Email;

                if (request.SoDienThoai != null)
                    manufacturer.SoDienThoai = request.SoDienThoai;

                if (request.Website != null)
                    manufacturer.Website = request.Website;

                manufacturer.TrangThai = request.TrangThai;
                manufacturer.SuaDoi = DateTime.Now;

                _context.NhaSanXuats.Update(manufacturer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật nhà sản xuất thành công",
                    manufacturer = new
                    {
                        manufacturer.MaNhaSX,
                        manufacturer.TenNhaSX,
                        manufacturer.DatNuoc,
                        manufacturer.Email,
                        manufacturer.SoDienThoai,
                        manufacturer.Website,
                        manufacturer.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/manufacturer/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteManufacturer(int id)
        {
            try
            {
                var manufacturer = await _context.NhaSanXuats.FindAsync(id);
                if (manufacturer == null)
                    return NotFound(new { message = "Nhà sản xuất không tồn tại" });

                // Check if manufacturer has products
                var hasProducts = await _context.SanPhams
                    .AnyAsync(s => s.MaNhaSX == id);

                if (hasProducts)
                    return BadRequest(new { message = "Không thể xóa nhà sản xuất vì có sản phẩm đang sử dụng" });

                _context.NhaSanXuats.Remove(manufacturer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa nhà sản xuất thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class CreateManufacturerRequest
        {
            public string TenNhaSX { get; set; } = string.Empty;
            public string? DatNuoc { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public string? Website { get; set; }
            public bool TrangThai { get; set; } = true;
        }

        public class UpdateManufacturerRequest
        {
            public string? TenNhaSX { get; set; }
            public string? DatNuoc { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public string? Website { get; set; }
            public bool TrangThai { get; set; } = true;
        }
    }
}