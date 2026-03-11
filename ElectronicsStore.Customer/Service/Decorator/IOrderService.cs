namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Component interface - Decorator Pattern
    /// Định nghĩa interface chung cho tất cả các dịch vụ đơn hàng
    /// </summary>
    public interface IOrderService
    {
        decimal GetTotalPrice();
        string GetDescription();
    }
}