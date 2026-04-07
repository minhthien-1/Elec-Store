using System;

namespace ElectronicsStore.Customer.Service.Adapter
{
    public class OldEmailSystem
    {
        // Tên phương thức khác hoàn toàn với chuẩn hiện tại
        public void ExecuteEmailSending(string content)
        {
            Console.WriteLine($"[Legacy Email]: Đang gửi nội dung: '{content}' qua SMTP cũ...");
        }
    }
}