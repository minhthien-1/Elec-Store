namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Builder helper - Giúp chain nhiều decorator dễ dàng hơn
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

        /// <summary>Thêm voucher từ DB (PhanTram hoặc TienCoThuc)</summary>
        public OrderServiceBuilder WithVoucher(string maCode, string kieuGiam, decimal giaTriGiam, decimal? giaTriGiamToiDa = null)
        {
            _service = new VoucherDecorator(_service, maCode, kieuGiam, giaTriGiam, giaTriGiamToiDa);
            return this;
        }

        public IOrderService Build() => _service;
    }
}