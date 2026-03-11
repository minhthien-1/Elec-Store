namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Concrete Decorator: Gói quà
    /// Thêm phí gói quà vào đơn hàng mà không thay đổi class gốc
    /// </summary>
    public class GiftWrapDecorator : OrderServiceDecorator
    {
        private const decimal GiftWrapFee = 50_000m; // 50.000 VNĐ

        public GiftWrapDecorator(IOrderService orderService)
            : base(orderService) { }

        public override decimal GetTotalPrice()
            => _wrappedService.GetTotalPrice() + GiftWrapFee;

        public override string GetDescription()
            => $"{_wrappedService.GetDescription()} + Gói quà (+{GiftWrapFee:N0} VNĐ)";
    }
}