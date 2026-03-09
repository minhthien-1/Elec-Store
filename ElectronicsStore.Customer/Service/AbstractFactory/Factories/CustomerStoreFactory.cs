using ElectronicsStore.AbstractFactory.ConcreteProducts;
using ElectronicsStore.AbstractFactory.Products;

namespace ElectronicsStore.AbstractFactory.Factories
{
    /// <summary>
    /// Concrete Factory: Customer Side
    /// Tạo ra sản phẩm với thông tin hiển thị phù hợp cho khách hàng
    /// (Giá đã bao gồm VAT, mô tả thân thiện, hình ảnh nổi bật...)
    /// </summary>
    public class CustomerStoreFactory : IStoreFactory
    {
        public IElectronicProduct CreateElectronicProduct(string type)
        {
            return type.ToLower() switch
            {
                "laptop" => new Laptop
                {
                    CPU = "Intel Core i7",
                    RAM = "16GB",
                    // Customer side: giá đã VAT, hiển thị thân thiện
                },
                "phone" => new Phone
                {
                    ManHinh = "6.7 inch AMOLED",
                    Pin = "5000",
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
                    ChuanKetNoi = "USB-C",
                },
                "sac" or "charger" => new ChargerAccessory
                {
                    CongSuat = 65,
                },
                _ => throw new ArgumentException($"Loại phụ kiện không hỗ trợ: {type}")
            };
        }
    }
}