namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Concrete Component - Đơn hàng cơ bản (không có dịch vụ bổ sung)
    /// </summary>
    public class BaseOrderService : IOrderService
    {
        private readonly string _productName;
        private readonly decimal _basePrice;

        public BaseOrderService(string productName, decimal basePrice)
        {
            _productName = productName;
            _basePrice = basePrice;
        }

        public decimal GetTotalPrice() => _basePrice;

        public string GetDescription() => $"{_productName} (Giá gốc: {_basePrice:N0} VNĐ)";
    }
}