namespace ElectronicsStore.Customer.Service.Pricing
{
    public interface IPricingStrategy
    {
        string GetStrategyName();
        // Tính phí vận chuyển dựa trên tổng tiền hàng
        decimal CalculateShippingFee(decimal subTotal);
        decimal CalculateDiscount(decimal subTotal);

        // Bạn có thể thêm CalculateDiscount(decimal subTotal, string voucherCode) ở đây sau này
    }
}