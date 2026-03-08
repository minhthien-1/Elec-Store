using ElectronicsStore.AbstractFactory.Products;

namespace ElectronicsStore.AbstractFactory.ConcreteProducts
{
    // ── ELECTRONICS ───────────────────────────────────────────────────────────

    public class Laptop : IElectronicProduct
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string DanhMuc => "Laptop";
        public string CPU { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;

        public string GetDisplayInfo()
            => $"[LAPTOP] {TenSP} | CPU: {CPU} | RAM: {RAM} | Giá: {GiaBan:N0} VNĐ";
    }

    public class Phone : IElectronicProduct
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string DanhMuc => "Phone";
        public string ManHinh { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;

        public string GetDisplayInfo()
            => $"[PHONE] {TenSP} | Màn hình: {ManHinh} | Pin: {Pin}mAh | Giá: {GiaBan:N0} VNĐ";
    }

    // ── ACCESSORIES ───────────────────────────────────────────────────────────

    public class CableAccessory : IAccessoryProduct
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string LoaiPhuKien => "Cáp";
        public string ChuanKetNoi { get; set; } = "USB-C"; // USB-C | Lightning | Micro-USB

        public string GetCompatibility()
            => $"Cáp {ChuanKetNoi} - Tương thích với các thiết bị dùng cổng {ChuanKetNoi}";
    }

    public class ChargerAccessory : IAccessoryProduct
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string LoaiPhuKien => "Sạc";
        public int CongSuat { get; set; } = 65; // Watt

        public string GetCompatibility()
            => $"Sạc {CongSuat}W - Hỗ trợ sạc nhanh cho Laptop/Phone tương thích";
    }
}