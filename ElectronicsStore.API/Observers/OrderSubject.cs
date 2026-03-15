using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.API.Observers
{
    public class OrderSubject
    {
        private readonly IEnumerable<IOrderObserver> _observers;

        public OrderSubject(IEnumerable<IOrderObserver> observers)
        {
            _observers = observers;
        }

        public async Task NotifyOrderCreated(DonHang order)
        {
            Console.WriteLine("SUBJECT: Notifying observers...");

            foreach (var observer in _observers)
            {
                await observer.OnOrderCreated(order);
            }

            Console.WriteLine("SUBJECT: All observers finished");
        }
    }
}