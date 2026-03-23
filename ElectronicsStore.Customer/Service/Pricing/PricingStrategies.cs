namespace ElectronicsStore.Customer.Service.Pricing
{
    // 1. CHIẾN LƯỢC KHÁCH THƯỜNG
    public class NormalPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Khách Thường (Không giảm giá)";
        
        public decimal CalculateShippingFee(decimal subTotal, string province)
        {
            // Ưu tiên 1: Đơn hàng lớn freeship toàn quốc
            if (subTotal >= 1000000) return 0;

            // Ưu tiên 2: Tính theo vùng địa lý
            if (string.IsNullOrEmpty(province)) return 30000;
            if (province.Contains("Hồ Chí Minh")) return 15000;
            if (province.Contains("Hà Giang") || province.Contains("Lào Cai")) return 50000;

            return 30000; // Mặc định các tỉnh khác
        }
        
        public decimal CalculateDiscount(decimal subTotal) => 0; 
    }

    // 2. CHIẾN LƯỢC VIP
    public class VIPPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Khách VIP (Giảm 10%, Freeship)";
        // VIP thì luôn freeship bất kể địa chỉ
        public decimal CalculateShippingFee(decimal subTotal, string province) => 0; 
        public decimal CalculateDiscount(decimal subTotal) => subTotal * 0.10m; 
    }

    // 3. CHIẾN LƯỢC NGÀY LỄ
    public class HolidayPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Ngày Lễ (Giảm 20%)";
        public decimal CalculateShippingFee(decimal subTotal, string province)
        {
            if (subTotal >= 1000000) return 0;
            if (string.IsNullOrEmpty(province)) return 30000;
            if (province.Contains("Hồ Chí Minh")) return 15000;
            return 30000;
        }
        public decimal CalculateDiscount(decimal subTotal) => subTotal * 0.20m; 
    }
}