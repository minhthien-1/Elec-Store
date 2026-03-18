using ElectronicsStore.API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectronicsStore.API.Observers
{
    public class OrderSubject
    {
        // Danh sách lưu trữ các Observer đang lắng nghe
        private readonly List<IOrderObserver> _observers = new List<IOrderObserver>();

        // Phương thức để đăng ký một Observer mới
        public void Attach(IOrderObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        // Phương thức để hủy đăng ký Observer
        public void Detach(IOrderObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        // Phương thức gửi thông báo tới tất cả các Observer đã đăng ký
        public async Task NotifyOrderCreated(DonHang order)
        {
            foreach (var observer in _observers)
            {
                await observer.OnOrderCreated(order);
            }
        }

    }
}