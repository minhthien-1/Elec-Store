using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using System.Text.Json;

namespace ElectronicsStore.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public OrderController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            if (request == null || request.OrderId <= 0 || string.IsNullOrEmpty(request.NewStatus))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var donHang = await _context.DonHangs.FindAsync(request.OrderId);
            if (donHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Lưu lịch sử đơn hàng
            var lichSu = new LichSuDonHang
            {
                MaDH = donHang.MaDH,
                TrangThaiCu = donHang.TrangThaiDon,
                TrangThaiMoi = request.NewStatus,
                MaNguoiCapNhat = donHang.MaND,
                LyDo = $"Admin cập nhật trạng thái sang {request.NewStatus}",
                NgayCapNhat = DateTime.UtcNow
            };
            _context.LichSuDonHangs.Add(lichSu);

            // Cập nhật trạng thái mới
            donHang.TrangThaiDon = request.NewStatus;
            donHang.NgayCapNhat = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Đã chuyển trạng thái đơn #{request.OrderId} thành {request.NewStatus}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật DB: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public int OrderId { get; set; }
        public string? NewStatus { get; set; }
    }
}
