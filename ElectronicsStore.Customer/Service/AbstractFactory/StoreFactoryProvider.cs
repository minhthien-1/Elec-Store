using ElectronicsStore.AbstractFactory.Factories;

namespace ElectronicsStore.AbstractFactory
{
    /// <summary>
    /// Factory Provider - Resolve đúng factory theo context (Admin/Customer)
    /// Đăng ký vào DI container trong Program.cs
    /// </summary>
    public static class StoreFactoryProvider
    {
        public static IStoreFactory GetFactory(string side)
        {
            return side.ToLower() switch
            {
                "admin" => new AdminStoreFactory(),
                "customer" => new CustomerStoreFactory(),
                _ => throw new ArgumentException($"Side không hợp lệ: {side}. Dùng 'admin' hoặc 'customer'.")
            };
        }
    }

    // -------------------------------------------------------------------------
    // USAGE EXAMPLE (trong ProductController.cs - Customer side):
    // -------------------------------------------------------------------------
    //
    //  // Lấy đúng factory theo side
    //  var factory = StoreFactoryProvider.GetFactory("customer");
    //
    //  // Tạo sản phẩm Laptop mới (để hiển thị cho khách)
    //  var laptop = factory.CreateElectronicProduct("laptop");
    //  laptop.MaSP   = 101;
    //  laptop.TenSP  = "Dell XPS 15";
    //  laptop.GiaBan = 30_000_000m;
    //
    //  Console.WriteLine(laptop.GetDisplayInfo());
    //  // [LAPTOP] Dell XPS 15 | CPU: Intel Core i7 | RAM: 16GB | Giá: 30.000.000 VNĐ
    //
    //  // Tạo phụ kiện đi kèm
    //  var charger = factory.CreateAccessory("charger");
    //  charger.TenSP  = "Sạc Dell 65W";
    //  charger.GiaBan = 500_000m;
    //
    // -------------------------------------------------------------------------
    // USAGE EXAMPLE (trong ProductController.cs - Admin side):
    // -------------------------------------------------------------------------
    //
    //  var factory = StoreFactoryProvider.GetFactory("admin");
    //  var newPhone = factory.CreateElectronicProduct("phone");
    //  // Admin nhận object với default "Chưa nhập" → bind vào form Create
    //
    // -------------------------------------------------------------------------
    // DI REGISTRATION (Program.cs):
    // -------------------------------------------------------------------------
    //
    //  // Option A: Đăng ký theo HttpContext (auto detect Admin/Customer)
    //  builder.Services.AddScoped<IStoreFactory>(sp =>
    //  {
    //      var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    //      bool isAdmin = httpContext?.Request.Path.StartsWithSegments("/admin") ?? false;
    //      return StoreFactoryProvider.GetFactory(isAdmin ? "admin" : "customer");
    //  });
    //
    //  // Option B: Đăng ký 2 factory có tên (Named services via keyed DI .NET 8)
    //  builder.Services.AddKeyedScoped<IStoreFactory, CustomerStoreFactory>("customer");
    //  builder.Services.AddKeyedScoped<IStoreFactory, AdminStoreFactory>("admin");
    //
    // -------------------------------------------------------------------------
}