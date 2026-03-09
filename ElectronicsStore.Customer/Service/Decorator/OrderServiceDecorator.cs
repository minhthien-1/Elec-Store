namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Base Decorator - Bọc quanh IOrderService, delegate mặc định về wrapped object
    /// </summary>
    public abstract class OrderServiceDecorator : IOrderService
    {
        protected readonly IOrderService _wrappedService;

        protected OrderServiceDecorator(IOrderService orderService)
        {
            _wrappedService = orderService;
        }

        public virtual decimal GetTotalPrice() => _wrappedService.GetTotalPrice();

        public virtual string GetDescription() => _wrappedService.GetDescription();
    }
}