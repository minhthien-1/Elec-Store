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

        public CreateOrderCommand(ElectronicsStoreDbContext context, OrderSubject subject)
        {
            _context = context;
            _subject = subject;
        }

        public async Task<int> ExecuteAsync(DonHang donHang)
        {
            Console.WriteLine("COMMAND PATTERN RUNNING");

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Order {donHang.MaDH} created");

            await _subject.NotifyOrderCreated(donHang);

            return donHang.MaDH;
        }
    }
}