namespace ElectronicsStore.Customer.Decorator
{
    /// <summary>
    /// Concrete Decorator: Voucher từ DB
    /// Hỗ trợ 2 kiểu giảm: "PhanTram" (%) và "TienCoThuc" (tiền mặt)
    /// </summary>
    public class VoucherDecorator : OrderServiceDecorator
    {
        private readonly string _maCode;
        private readonly string _kieuGiam;
        private readonly decimal _giaTriGiam;
        private readonly decimal? _giaTriGiamToiDa;

        public VoucherDecorator(
            IOrderService orderService,
            string maCode,
            string kieuGiam,
            decimal giaTriGiam,
            decimal? giaTriGiamToiDa = null)
            : base(orderService)
        {
            _maCode = maCode;
            _kieuGiam = kieuGiam;
            _giaTriGiam = giaTriGiam;
            _giaTriGiamToiDa = giaTriGiamToiDa;
        }

        public override decimal GetTotalPrice()
        {
            var original = _wrappedService.GetTotalPrice();
            var discount = TinhTienGiam(original);
            return original - discount;
        }

        public override string GetDescription()
        {
            var original = _wrappedService.GetTotalPrice();
            var discount = TinhTienGiam(original);

            var moTaGiam = _kieuGiam == "PhanTram"
                ? $"{_giaTriGiam}%{(_giaTriGiamToiDa.HasValue ? $" (tối đa {_giaTriGiamToiDa:N0} VNĐ)" : "")}"
                : $"{_giaTriGiam:N0} VNĐ";

            return $"{_wrappedService.GetDescription()} + Voucher [{_maCode}] ({moTaGiam}: -{discount:N0} VNĐ)";
        }

        private decimal TinhTienGiam(decimal original)
        {
            decimal discount = _kieuGiam == "PhanTram"
                ? original * _giaTriGiam / 100
                : _giaTriGiam;

            if (_giaTriGiamToiDa.HasValue && discount > _giaTriGiamToiDa.Value)
                discount = _giaTriGiamToiDa.Value;

            return discount;
        }
    }
}