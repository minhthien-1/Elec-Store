using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;

        public PromotionController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /api/promotion
        [HttpGet]
        public async Task<ActionResult<object>> GetAllPromotions()
        {
            try
            {
                var promotions = await _context.MaKhuyenMais
                    .Where(p => p.TrangThai == true && (p.NgayKetThuc == null || p.NgayKetThuc >= DateTime.Now))
                    .Select(p => new
                    {
                        p.MaKM,
                        p.MaCode,
                        p.TenChienDich,
                        p.MoTa,
                        p.KieuGiam,
                        p.GiaTriGiam,
                        p.GiaTriGiamToiDa,
                        p.SoLuotSuDung,
                        p.GioiHanSoLuotSuDung,
                        p.GiaTriDonHangToiThieu,
                        p.NgayBatDau,
                        p.NgayKetThuc
                    })
                    .ToListAsync();

                return Ok(new { total = promotions.Count, data = promotions });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/promotion/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPromotionById(int id)
        {
            try
            {
                var promotion = await _context.MaKhuyenMais.FindAsync(id);
                if (promotion == null)
                    return NotFound(new { message = "Mã khuyến mãi không tồn tại" });

                return Ok(new
                {
                    maKM = promotion.MaKM,
                    maCode = promotion.MaCode,
                    tenChienDich = promotion.TenChienDich,
                    moTa = promotion.MoTa,
                    kieuGiam = promotion.KieuGiam,
                    giaTriGiam = promotion.GiaTriGiam,
                    giaTriGiamToiDa = promotion.GiaTriGiamToiDa,
                    soLuotSuDung = promotion.SoLuotSuDung,
                    gioiHanSoLuotSuDung = promotion.GioiHanSoLuotSuDung,
                    giaTriDonHangToiThieu = promotion.GiaTriDonHangToiThieu,
                    ngayBatDau = promotion.NgayBatDau,
                    ngayKetThuc = promotion.NgayKetThuc,
                    trangThai = promotion.TrangThai
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/promotion/validate/{maCode}
        [HttpGet("validate/{maCode}")]
        public async Task<ActionResult<object>> ValidatePromoCode(string maCode, [FromQuery] decimal tongGia)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maCode))
                    return BadRequest(new { message = "Mã khuyến mãi không được để trống" });

                var promotion = await _context.MaKhuyenMais
                    .FirstOrDefaultAsync(p => p.MaCode.ToLower() == maCode.ToLower() && p.TrangThai == true);

                if (promotion == null)
                    return NotFound(new { message = "Mã khuyến mãi không hợp lệ" });

                // Check expiration
                if (promotion.NgayBatDau > DateTime.Now)
                    return BadRequest(new { message = "Mã khuyến mãi chưa được kích hoạt" });

                if (promotion.NgayKetThuc < DateTime.Now)
                    return BadRequest(new { message = "Mã khuyến mãi đã hết hạn" });

                // Check usage limit
                if (promotion.GioiHanSoLuotSuDung.HasValue && promotion.SoLuotSuDung >= promotion.GioiHanSoLuotSuDung)
                    return BadRequest(new { message = "Mã khuyến mãi đã hết lượt sử dụng" });

                // Check minimum order value
                if (promotion.GiaTriDonHangToiThieu.HasValue && tongGia < promotion.GiaTriDonHangToiThieu)
                    return BadRequest(new
                    {
                        message = $"Đơn hàng phải tối thiểu {promotion.GiaTriDonHangToiThieu}",
                        minValue = promotion.GiaTriDonHangToiThieu
                    });

                // Calculate discount
                decimal discount = 0;
                if (promotion.KieuGiam == "PhanTram")
                {
                    discount = (tongGia * promotion.GiaTriGiam.GetValueOrDefault()) / 100;
                }
                else if (promotion.KieuGiam == "TienCoThuc")
                {
                    discount = promotion.GiaTriGiam.GetValueOrDefault();
                }

                // Apply max discount cap if exists
                if (promotion.GiaTriGiamToiDa.HasValue && discount > promotion.GiaTriGiamToiDa)
                    discount = promotion.GiaTriGiamToiDa.Value;

                var finalPrice = tongGia - discount;

                return Ok(new
                {
                    valid = true,
                    maCode = promotion.MaCode,
                    kieuGiam = promotion.KieuGiam,
                    giaTriGiam = promotion.GiaTriGiam,
                    giaTriGiamToiDa = promotion.GiaTriGiamToiDa,
                    discount,
                    finalPrice,
                    message = "Mã khuyến mãi hợp lệ"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/promotion
        [HttpPost]
        public async Task<ActionResult<object>> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MaCode))
                    return BadRequest(new { message = "Mã code không được để trống" });

                var existingCode = await _context.MaKhuyenMais
                    .FirstOrDefaultAsync(p => p.MaCode.ToLower() == request.MaCode.ToLower());

                if (existingCode != null)
                    return BadRequest(new { message = "Mã code đã tồn tại" });

                var promotion = new MaKhuyenMai
                {
                    MaCode = request.MaCode.ToUpper(),
                    TenChienDich = request.TenChienDich,
                    MoTa = request.MoTa,
                    KieuGiam = request.KieuGiam,
                    GiaTriGiam = request.GiaTriGiam,
                    GiaTriGiamToiDa = request.GiaTriGiamToiDa,
                    SoLuotSuDung = 0,
                    GioiHanSoLuotSuDung = request.GioiHanSoLuotSuDung,
                    GiaTriDonHangToiThieu = request.GiaTriDonHangToiThieu,
                    NgayBatDau = request.NgayBatDau,
                    NgayKetThuc = request.NgayKetThuc,
                    TrangThai = request.TrangThai,
                    ThemTrongDB = DateTime.Now
                };

                _context.MaKhuyenMais.Add(promotion);
                await _context.SaveChangesAsync();

                return Created($"api/promotion/{promotion.MaKM}", new
                {
                    message = "Tạo mã khuyến mãi thành công",
                    promotion = new
                    {
                        promotion.MaKM,
                        promotion.MaCode,
                        promotion.TenChienDich,
                        promotion.KieuGiam,
                        promotion.GiaTriGiam
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/promotion/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
        {
            try
            {
                var promotion = await _context.MaKhuyenMais.FindAsync(id);
                if (promotion == null)
                    return NotFound(new { message = "Mã khuyến mãi không tồn tại" });

                if (!string.IsNullOrWhiteSpace(request.TenChienDich))
                    promotion.TenChienDich = request.TenChienDich;

                if (request.MoTa != null)
                    promotion.MoTa = request.MoTa;

                if (request.GiaTriGiam.HasValue)
                    promotion.GiaTriGiam = request.GiaTriGiam;

                if (request.GiaTriGiamToiDa.HasValue)
                    promotion.GiaTriGiamToiDa = request.GiaTriGiamToiDa;

                if (request.GioiHanSoLuotSuDung.HasValue)
                    promotion.GioiHanSoLuotSuDung = request.GioiHanSoLuotSuDung;

                if (request.GiaTriDonHangToiThieu.HasValue)
                    promotion.GiaTriDonHangToiThieu = request.GiaTriDonHangToiThieu;

                if (request.NgayKetThuc.HasValue)
                    promotion.NgayKetThuc = request.NgayKetThuc;

                promotion.TrangThai = request.TrangThai;

                _context.MaKhuyenMais.Update(promotion);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật mã khuyến mãi thành công",
                    promotion = new
                    {
                        promotion.MaKM,
                        promotion.MaCode,
                        promotion.TenChienDich,
                        promotion.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/promotion/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeletePromotion(int id)
        {
            try
            {
                var promotion = await _context.MaKhuyenMais.FindAsync(id);
                if (promotion == null)
                    return NotFound(new { message = "Mã khuyến mãi không tồn tại" });

                _context.MaKhuyenMais.Remove(promotion);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa mã khuyến mãi thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class CreatePromotionRequest
        {
            public string MaCode { get; set; } = string.Empty;
            public string? TenChienDich { get; set; }
            public string? MoTa { get; set; }
            public string? KieuGiam { get; set; } // 'PhanTram' hoặc 'TienCoThuc'
            public decimal? GiaTriGiam { get; set; }
            public decimal? GiaTriGiamToiDa { get; set; }
            public int? GioiHanSoLuotSuDung { get; set; }
            public decimal? GiaTriDonHangToiThieu { get; set; }
            public DateTime? NgayBatDau { get; set; }
            public DateTime? NgayKetThuc { get; set; }
            public bool TrangThai { get; set; } = true;
        }

        public class UpdatePromotionRequest
        {
            public string? TenChienDich { get; set; }
            public string? MoTa { get; set; }
            public decimal? GiaTriGiam { get; set; }
            public decimal? GiaTriGiamToiDa { get; set; }
            public int? GioiHanSoLuotSuDung { get; set; }
            public decimal? GiaTriDonHangToiThieu { get; set; }
            public DateTime? NgayKetThuc { get; set; }
            public bool TrangThai { get; set; } = true;
        }
    }
}