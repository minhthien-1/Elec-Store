using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Customer.Service.Payment
{
    public interface IPayment
    {
        string GeneratePaymentUrl(DonHang order, HttpContext httpContext);
    }
}