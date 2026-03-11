namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Builder helper - Giúp chain nhiều decorator dễ dàng hơn
    /// Sử dụng trong Controller hoặc Service layer
    /// </summary>
    public class OrderServiceBuilder
    {
        private IOrderService _service;

        public OrderServiceBuilder(string productName, decimal basePrice)
        {
            _service = new BaseOrderService(productName, basePrice);
        }

        /// <summary>Thêm dịch vụ gói quà (+50.000 VNĐ)</summary>
        public OrderServiceBuilder WithGiftWrap()
        {
            _service = new GiftWrapDecorator(_service);
            return this;
        }

        /// <summary>Thêm giảm giá thành viên theo hạng (Silver/Gold/Platinum)</summary>
        public OrderServiceBuilder WithMemberDiscount(string memberTier)
        {
            _service = new MemberDiscountDecorator(_service, memberTier);
            return this;
        }

        public IOrderService Build() => _service;
    }

    // -------------------------------------------------------------------------
    // USAGE EXAMPLE (trong CartController.cs hoặc CheckoutController.cs):
    // -------------------------------------------------------------------------
    //
    //  var order = new OrderServiceBuilder("Laptop Dell XPS 15", 30_000_000m)
    //                  .WithGiftWrap()
    //                  .WithMemberDiscount("Gold")
    //                  .Build();
    //
    //  ViewBag.TotalPrice   = order.GetTotalPrice();   // 27.050.000 VNĐ
    //  ViewBag.Description  = order.GetDescription();
    //  // Output: "Laptop Dell XPS 15 (Giá gốc: 30.000.000 VNĐ)
    //  //          + Gói quà (+50.000 VNĐ)
    //  //          + Giảm giá Gold (10%: -3.000.000 VNĐ)"
    // -------------------------------------------------------------------------
}