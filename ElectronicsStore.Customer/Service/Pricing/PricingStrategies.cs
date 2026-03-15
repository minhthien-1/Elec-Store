namespace ElectronicsStore.Customer.Service.Pricing
{
    // 1. CHIẾN LƯỢC KHÁCH THƯỜNG
    public class NormalPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Khách Thường (Không giảm giá)";
        public decimal CalculateShippingFee(decimal subTotal) => subTotal >= 1000000 ? 0 : 30000;
        public decimal CalculateDiscount(decimal subTotal) => 0; 
    }

    // 2. CHIẾN LƯỢC VIP (Giảm 10%, luôn Freeship)
    public class VIPPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Khách VIP (Giảm 10%, Freeship)";
        public decimal CalculateShippingFee(decimal subTotal) => 0; 
        public decimal CalculateDiscount(decimal subTotal) => subTotal * 0.10m; 
    }

    // 3. CHIẾN LƯỢC NGÀY LỄ (Giảm 20%)
    public class HolidayPricingStrategy : IPricingStrategy
    {
        public string GetStrategyName() => "Ngày Lễ (Giảm 20%)";
        public decimal CalculateShippingFee(decimal subTotal) => subTotal >= 1000000 ? 0 : 30000;
        public decimal CalculateDiscount(decimal subTotal) => subTotal * 0.20m; 
    }
}