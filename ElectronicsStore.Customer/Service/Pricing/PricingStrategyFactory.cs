using System;

namespace ElectronicsStore.Customer.Service.Pricing
{
    public class PricingStrategyFactory
    {
        public IPricingStrategy CreateStrategy(decimal subTotal)
        {
            // 1. Ưu tiên 1: Ngày lễ (Ví dụ: Tháng 3 ngày 8, hoặc Quốc khánh...)
            // Bạn có thể test bằng cách đổi thành ngày hôm nay
            if (DateTime.Now.Month == 3 && DateTime.Now.Day == 8)
            {
                return new HolidayPricingStrategy();
            }

            // 2. Ưu tiên 2: Khách mua nhiều (Mua trên 30.000.000đ thì tự động thành VIP)
            if (subTotal >= 30000000)
            {
                return new VIPPricingStrategy();
            }

            // 3. Mặc định: Khách thường
            return new NormalPricingStrategy();
        }
    }
}