namespace ElectronicsStore.AbstractFactory.Products
{
    /// <summary>
    /// Abstract Product A - Sản phẩm điện tử
    /// </summary>
    public interface IElectronicProduct
    {
        int MaSP { get; set; }
        string TenSP { get; set; }
        decimal GiaBan { get; set; }
        string DanhMuc { get; }        // "Laptop" | "Phone"
        string GetDisplayInfo();
    }

    /// <summary>
    /// Abstract Product B - Phụ kiện
    /// </summary>a
    public interface IAccessoryProduct
    {
        int MaSP { get; set; }
        string TenSP { get; set; }
        decimal GiaBan { get; set; }
        string LoaiPhuKien { get; }    // "Cáp" | "Sạc" | "Tai nghe"
        string GetCompatibility();
    }
}