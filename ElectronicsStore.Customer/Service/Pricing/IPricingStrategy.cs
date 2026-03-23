namespace ElectronicsStore.Customer.Service.Pricing
{
    public interface IPricingStrategy
    {
        string GetStrategyName();
        // Cập nhật: Thêm tham số province
        decimal CalculateShippingFee(decimal subTotal, string province);
        decimal CalculateDiscount(decimal subTotal);
    }
}