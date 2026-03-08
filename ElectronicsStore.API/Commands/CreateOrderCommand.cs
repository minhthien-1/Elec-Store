using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using ElectronicsStore.API.Observers;

namespace ElectronicsStore.API.Commands
{
    public class CreateOrderCommand
    {
        private readonly ElectronicsStoreDbContext _context;
        private readonly DonHang _donHang;
        private readonly OrderSubject _subject;

        public CreateOrderCommand(
            ElectronicsStoreDbContext context,
            DonHang donHang,
            OrderSubject subject)
        {
            _context = context;
            _donHang = donHang;
            _subject = subject;
        }

        public async Task ExecuteAsync()
        {
            _context.DonHangs.Add(_donHang);
            await _context.SaveChangesAsync();

            await _subject.NotifyOrderCreated(_donHang);
        }
    }
}