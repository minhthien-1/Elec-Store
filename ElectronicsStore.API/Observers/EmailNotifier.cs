using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.API.Observers;

public class EmailNotifier : IOrderObserver
{
    public Task OnOrderCreated(DonHang order)
    {
        Console.WriteLine($"Send email for order {order.MaDH}");
        return Task.CompletedTask;
    }
}