using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Customer.Models
{
    public class CheckoutViewModel
    {
        // Thông tin người nhận
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChiCuThe { get; set; } // Số nhà, tên đường

        // 3 trường này nhận từ API địa chính
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string PhuongXa { get; set; }

        public string GhiChu { get; set; }
        public string HinhThucThanhToan { get; set; } // "COD" hoặc "VNPAY"

        // Dữ liệu hiển thị (Read-only)
        public List<GioHang> CartItems { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TongThanhToan => TongTienHang + PhiVanChuyen;
    }
}