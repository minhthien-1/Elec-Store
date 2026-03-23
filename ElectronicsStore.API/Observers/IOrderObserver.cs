using ElectronicsStore.API.Models.Entities;
using System;
using System.Threading.Tasks;

namespace ElectronicsStore.API.Observers
{
    public interface IOrderObserver
    {
        Task OnOrderCreated(DonHang order); // Phương thức 1
        void OnOrderCreated(string orderInfo); // Phương thức 2
    }

    public class TerminalLoggerObserver : IOrderObserver
    {
        // Triển khai phương thức nhận vào object DonHang (Sửa lỗi CS0535)
        public Task OnOrderCreated(DonHang order)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n======= [COMMAND & OBSERVER SUCCESS] =======");
            Console.WriteLine($"[TERMINAL]: Đã kết nối API thành công!");
            Console.WriteLine($"[ORDER]: Đơn hàng mã số {order.MaDH} đã được tạo.");
            Console.WriteLine("============================================\n");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        // Triển khai phương thức nhận vào string (Bạn đã có)
        public void OnOrderCreated(string orderInfo)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[--- OBSERVER NOTIFICATION ---]");
            Console.WriteLine($"[LOG]: Phát hiện đơn hàng mới từ Customer!");
            Console.WriteLine($"[DATA]: {orderInfo}");
            Console.WriteLine($"[TIME]: {DateTime.Now}");
            Console.WriteLine("[-----------------------------]\n");
            Console.ResetColor();
        }
    }
}