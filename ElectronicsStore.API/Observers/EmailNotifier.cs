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
        public void OnOrderCreated(string orderInfo)
        {
            // Nếu không dùng đến, bạn có thể để trống hoặc in ra terminal
            Console.WriteLine($"[EMAIL OBSERVER]: Đang chuẩn bị gửi thông báo cho: {orderInfo}");
        }
    }
}