using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.API.Observers
{
    public class EmailNotifier : IOrderObserver
    {
        public Task OnOrderCreated(DonHang order)
        {
            Console.WriteLine("[Observer] EmailNotifier triggered");
            Console.WriteLine($"Sending email for order {order.MaDH}");

            return Task.CompletedTask;
        }
    }
}