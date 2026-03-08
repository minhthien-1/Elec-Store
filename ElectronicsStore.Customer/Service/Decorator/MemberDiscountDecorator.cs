namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Concrete Decorator: Giảm giá thành viên
    /// Áp dụng % giảm giá theo hạng thành viên (Silver/Gold/Platinum)
    /// </summary>
    public class MemberDiscountDecorator : OrderServiceDecorator
    {
        private readonly decimal _discountPercent;
        private readonly string _memberTier;

        // Giảm giá theo hạng thành viên
        private static readonly Dictionary<string, decimal> TierDiscounts = new()
        {
            { "Silver",   0.05m }, // 5%
            { "Gold",     0.10m }, // 10%
            { "Platinum", 0.15m }  // 15%
        };

        public MemberDiscountDecorator(IOrderService orderService, string memberTier)
            : base(orderService)
        {
            _memberTier = memberTier;
            _discountPercent = TierDiscounts.TryGetValue(memberTier, out var discount)
                ? discount
                : 0.05m; // Default: Silver
        }

        public override decimal GetTotalPrice()
        {
            var original = _wrappedService.GetTotalPrice();
            var discountAmount = original * _discountPercent;
            return original - discountAmount;
        }

        public override string GetDescription()
        {
            var original = _wrappedService.GetTotalPrice();
            var discountAmount = original * _discountPercent;
            return $"{_wrappedService.GetDescription()} + Giảm giá {_memberTier} ({_discountPercent * 100}%: -{discountAmount:N0} VNĐ)";
        }
    }
}