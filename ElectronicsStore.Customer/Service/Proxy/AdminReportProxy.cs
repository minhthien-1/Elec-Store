using System;

namespace ElectronicsStore.Customer.Service.Proxy
{
    public class AdminReportProxy : IReportService
    {
        private RealReportService _realService;
        private string _userRole;

        public AdminReportProxy(string userRole)
        {
            _userRole = userRole;
        }

        public void DisplayReport()
        {
            if (_userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (_realService == null) _realService = new RealReportService();
                _realService.DisplayReport();
            }
            else
            {
                Console.WriteLine("[PROXY]: Truy cập bị từ chối! Bạn không có quyền xem báo cáo này.");
            }
        }
    }
}