namespace ElectronicsStore.Customer.Service.Adapter
{
    public class EmailAdapter : INotificationService
    {
        private readonly OldEmailSystem _oldSystem;

        public EmailAdapter(OldEmailSystem oldSystem)
        {
            _oldSystem = oldSystem;
        }

        public void Send(string message)
        {
            // Chuyển đổi lời gọi từ hệ thống mới sang hệ thống cũ
            _oldSystem.ExecuteEmailSending(message);
        }
    }
}