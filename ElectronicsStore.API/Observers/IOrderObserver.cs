using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.API.Observers
{
    public interface IOrderObserver
    {
        Task OnOrderCreated(DonHang order);
    }
}