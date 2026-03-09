using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Customer.Service.Payment
{
    public class CodPayment : IPayment
    {
        public string GeneratePaymentUrl(DonHang order, HttpContext httpContext)
        {
            // Trả về thẳng đường dẫn nội bộ tới trang Success
            return $"/Checkout/Success?orderId={order.MaDH}";
        }
    }
}