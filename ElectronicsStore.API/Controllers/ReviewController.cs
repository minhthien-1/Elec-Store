using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ElectronicsStoreDbContext _context;

        public ReviewController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        // GET: /api/review/product/{maSP}
        [HttpGet("product/{maSP}")]
        public async Task<ActionResult<object>> GetReviewsByProduct(int maSP)
        {
            try
            {
                var reviews = await _context.DanhGiaSanPhams
                    .Where(r => r.MaSP == maSP && r.DuocDuyet == true)
                    .Include(r => r.NguoiDung)
                    .Select(r => new
                    {
                        r.MaDG,
                        r.MaSP,
                        r.DemSao,
                        r.TieuDe,
                        r.NoiDung,
                        r.CoDienTieuDung,
                        r.SoLuotThichChu,
                        r.ThemTrongDB,
                        NguoiDung = new
                        {
                            r.NguoiDung!.MaND,
                            r.NguoiDung.TenDayDu,
                            r.NguoiDung.Email
                        }
                    })
                    .OrderByDescending(r => r.ThemTrongDB)
                    .ToListAsync();

                return Ok(new { total = reviews.Count, data = reviews });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /api/review/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetReviewById(int id)
        {
            try
            {
                var review = await _context.DanhGiaSanPhams
                    .Where(r => r.MaDG == id)
                    .Include(r => r.NguoiDung)
                    .Include(r => r.SanPham)
                    .FirstOrDefaultAsync();

                if (review == null)
                    return NotFound(new { message = "Đánh giá không tồn tại" });

                return Ok(new
                {
                    maDG = review.MaDG,
                    maSP = review.MaSP,
                    maND = review.MaND,
                    demSao = review.DemSao,
                    tieuDe = review.TieuDe,
                    noiDung = review.NoiDung,
                    coDienTieuDung = review.CoDienTieuDung,
                    duocDuyet = review.DuocDuyet,
                    soLuotThichChu = review.SoLuotThichChu,
                    themTrongDB = review.ThemTrongDB,
                    suaDoi = review.SuaDoi,
                    nguoiDung = new
                    {
                        review.NguoiDung!.TenDayDu,
                        review.NguoiDung.Email
                    },
                    sanPham = new
                    {
                        review.SanPham!.TenSP
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /api/review
        [HttpPost]
        public async Task<ActionResult<object>> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                if (request.DemSao < 1 || request.DemSao > 5)
                    return BadRequest(new { message = "Số sao phải từ 1 đến 5" });

                var sanPham = await _context.SanPhams.FindAsync(request.MaSP);
                if (sanPham == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                var nguoiDung = await _context.NguoiDungs.FindAsync(request.MaND);
                if (nguoiDung == null)
                    return NotFound(new { message = "Người dùng không tồn tại" });

                var review = new DanhGiaSanPham
                {
                    MaSP = request.MaSP,
                    MaND = request.MaND,
                    DemSao = request.DemSao,
                    TieuDe = request.TieuDe,
                    NoiDung = request.NoiDung,
                    CoDienTieuDung = request.CoDienTieuDung,
                    DuocDuyet = false, // Cần admin duyệt
                    SoLuotThichChu = 0,
                    ThemTrongDB = DateTime.Now
                };

                _context.DanhGiaSanPhams.Add(review);
                await _context.SaveChangesAsync();

                return Created($"api/review/{review.MaDG}", new
                {
                    message = "Tạo đánh giá thành công, chờ admin duyệt",
                    review = new
                    {
                        review.MaDG,
                        review.MaSP,
                        review.DemSao,
                        review.TieuDe,
                        review.DuocDuyet
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/review/{id}/like
        [HttpPut("{id}/like")]
        public async Task<ActionResult<object>> LikeReview(int id)
        {
            try
            {
                var review = await _context.DanhGiaSanPhams.FindAsync(id);
                if (review == null)
                    return NotFound(new { message = "Đánh giá không tồn tại" });

                review.SoLuotThichChu += 1;
                _context.DanhGiaSanPhams.Update(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thích đánh giá thành công", soLuotThich = review.SoLuotThichChu });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/review/{id}/approve (Admin only)
        [HttpPut("{id}/approve")]
        public async Task<ActionResult<object>> ApproveReview(int id)
        {
            try
            {
                var review = await _context.DanhGiaSanPhams.FindAsync(id);
                if (review == null)
                    return NotFound(new { message = "Đánh giá không tồn tại" });

                review.DuocDuyet = true;
                review.SuaDoi = DateTime.Now;

                // Update product rating
                var sanPham = await _context.SanPhams.FindAsync(review.MaSP);
                if (sanPham != null)
                {
                    var allReviews = await _context.DanhGiaSanPhams
                        .Where(r => r.MaSP == review.MaSP && r.DuocDuyet == true)
                        .ToListAsync();

                    var avgRating = allReviews.Average(r => r.DemSao);
                    sanPham.DanhGiaXepHang = (decimal)avgRating;
                    sanPham.SoLuotDanhGia = allReviews.Count;
                    _context.SanPhams.Update(sanPham);
                }

                _context.DanhGiaSanPhams.Update(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Duyệt đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /api/review/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteReview(int id)
        {
            try
            {
                var review = await _context.DanhGiaSanPhams.FindAsync(id);
                if (review == null)
                    return NotFound(new { message = "Đánh giá không tồn tại" });

                _context.DanhGiaSanPhams.Remove(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi: {ex.Message}" });
            }
        }

        public class CreateReviewRequest
        {
            public int MaSP { get; set; }
            public int MaND { get; set; }
            public int DemSao { get; set; }
            public string? TieuDe { get; set; }
            public string? NoiDung { get; set; }
            public bool CoDienTieuDung { get; set; } = false;
        }
    }
}