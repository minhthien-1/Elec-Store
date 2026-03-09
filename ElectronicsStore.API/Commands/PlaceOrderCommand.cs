using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.API.Observers;

namespace ElectronicsStore.API.Commands
{
    public class PlaceOrderCommand : IOrderCommand
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly DonHang _order;
        private readonly OrderSubject _subject;

        public PlaceOrderCommand(ElectronicsStoreDbContext context, DonHang order, OrderSubject subject)
        {
            _context = context;
            _order = order;
            _subject = subject;
        }

        public async Task ExecuteAsync()
        {
            _context.DonHangs.Add(_order);
            await _context.SaveChangesAsync();

            // Trigger observer event
            await _subject.NotifyOrderCreated(_order);
        }
    }
}