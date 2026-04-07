using System;

namespace ElectronicsStore.Customer.Service.Proxy
{
    public class RealReportService : IReportService
    {
        public void DisplayReport()
        {
            Console.WriteLine("--- DỮ LIỆU DOANH THU CHI TIẾT (CONFIDENTIAL) ---");
            Console.WriteLine("Tổng doanh thu tháng 04/2026: 1,500,000,000 VND");
            Console.WriteLine("Lợi nhuận ròng: 450,000,000 VND");
        }
    }
}