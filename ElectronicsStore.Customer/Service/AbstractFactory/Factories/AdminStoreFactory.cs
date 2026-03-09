using ElectronicsStore.AbstractFactory.ConcreteProducts;
using ElectronicsStore.AbstractFactory.Products;

namespace ElectronicsStore.AbstractFactory.Factories
{
    /// <summary>
    /// Concrete Factory: Admin Side
    /// Tạo ra sản phẩm với thông tin quản trị (giá nhập, thông số kỹ thuật đầy đủ,
    /// mã kho, nhà cung cấp...)
    /// </summary>
    public class AdminStoreFactory : IStoreFactory
    {
        public IElectronicProduct CreateElectronicProduct(string type)
        {
            return type.ToLower() switch
            {
                "laptop" => new Laptop
                {
                    CPU = "Chưa nhập",
                    RAM = "Chưa nhập",
                    // Admin side: form nhập đầy đủ thông số
                },
                "phone" => new Phone
                {
                    ManHinh = "Chưa nhập",
                    Pin = "0",
                },
                _ => throw new ArgumentException($"Loại sản phẩm không hỗ trợ: {type}")
            };
        }

        public IAccessoryProduct CreateAccessory(string type)
        {
            return type.ToLower() switch
            {
                "cap" or "cable" => new CableAccessory
                {
                    ChuanKetNoi = "Chưa chọn",
                },
                "sac" or "charger" => new ChargerAccessory
                {
                    CongSuat = 0,
                },
                _ => throw new ArgumentException($"Loại phụ kiện không hỗ trợ: {type}")
            };
        }
    }
}