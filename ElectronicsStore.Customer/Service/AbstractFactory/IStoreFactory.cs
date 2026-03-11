using ElectronicsStore.AbstractFactory.Products;

namespace ElectronicsStore.AbstractFactory
{
    /// <summary>
    /// Abstract Factory Interface
    /// Mỗi "side" (Admin/Customer) implement factory này theo cách riêng.
    /// Nếu sau này thêm "Đồ gia dụng", chỉ cần tạo HomeApplianceFactory mới.
    /// </summary>
    public interface IStoreFactory
    {
        /// <summary>Tạo sản phẩm điện tử (Laptop hoặc Phone)</summary>
        IElectronicProduct CreateElectronicProduct(string type);

        /// <summary>Tạo phụ kiện (Cáp hoặc Sạc)</summary>
        IAccessoryProduct CreateAccessory(string type);
    }
}