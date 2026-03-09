using System.Text;
using ElectronicsStore.API.Data;
using ElectronicsStore.Customer.Service.Payment;
using ElectronicsStore.Customer.Service.Pricing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;



using ElectronicsStore.Customer.Decorator;
using ElectronicsStore.AbstractFactory;
using ElectronicsStore.AbstractFactory.Factories;
namespace ElectronicsStore.Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // --- 1. CẤU HÌNH DATABASE (POSTGRESQL) ---
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ElectronicsStoreDbContext>(options =>
                options.UseNpgsql(connectionString));

            // --- 2. CẤU HÌNH SERVICES ---
            builder.Services.AddControllersWithViews();

            // --- CẤU HÌNH ĐĂNG NHẬP ---
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Index";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Home/Index";
                    options.Cookie.Name = "ElectronicsStore_Session";
                });

           // Đăng ký 2 Factory vào hệ thống
builder.Services.AddScoped<PricingStrategyFactory>();
builder.Services.AddScoped<PaymentFactory>();

            var app = builder.Build();

            // --- 3. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // // =====================================================
            // // TEST DESIGN PATTERNS - XÓA SAU KHI DEMO XONG
            // // =====================================================
            // Console.WriteLine("\n========== DECORATOR PATTERN ==========");

            // // Test 1: Gói quà + Giảm giá Gold
            // var order1 = new OrderServiceBuilder("Samsung Galaxy S24 Ultra", 24_990_000m)
            //                 .WithGiftWrap()
            //                 .WithMemberDiscount("Gold")
            //                 .Build();
            // Console.WriteLine(order1.GetDescription());
            // Console.WriteLine($"=> Tổng tiền: {order1.GetTotalPrice():N0} VNĐ");

            // Console.WriteLine();

            // // Test 2: Chỉ gói quà
            // var order2 = new OrderServiceBuilder("Laptop Dell XPS 15", 30_000_000m)
            //                 .WithGiftWrap()
            //                 .Build();
            // Console.WriteLine(order2.GetDescription());
            // Console.WriteLine($"=> Tổng tiền: {order2.GetTotalPrice():N0} VNĐ");

            // Console.WriteLine();

            // // Test 3: Chỉ giảm giá Platinum
            // var order3 = new OrderServiceBuilder("iPhone 15 Pro Max", 29_990_000m)
            //                 .WithMemberDiscount("Platinum")
            //                 .Build();
            // Console.WriteLine(order3.GetDescription());
            // Console.WriteLine($"=> Tổng tiền: {order3.GetTotalPrice():N0} VNĐ");

            // Console.WriteLine("\n========== ABSTRACT FACTORY PATTERN ==========");

            // // Customer Factory
            // var customerFactory = StoreFactoryProvider.GetFactory("customer");
            // var phone = customerFactory.CreateElectronicProduct("phone");
            // phone.MaSP = 4; phone.TenSP = "Samsung Galaxy S24 Ultra"; phone.GiaBan = 24_990_000m;
            // Console.WriteLine("[CUSTOMER] " + phone.GetDisplayInfo());

            // var charger = customerFactory.CreateAccessory("charger");
            // charger.TenSP = "Sạc Samsung 65W"; charger.GiaBan = 500_000m;
            // Console.WriteLine("[CUSTOMER] " + charger.GetCompatibility());

            // Console.WriteLine();

            // // Admin Factory
            // var adminFactory = StoreFactoryProvider.GetFactory("admin");
            // var adminPhone = adminFactory.CreateElectronicProduct("phone");
            // adminPhone.TenSP = "Sản phẩm mới (chưa nhập)";
            // Console.WriteLine("[ADMIN] " + adminPhone.GetDisplayInfo());

            // Console.WriteLine("========================================\n");
            // // =====================================================
            // // HẾT PHẦN TEST
            // // =====================================================

            app.Run();
            builder.Services.AddHttpClient<ProductApiService>();
        }
        
    }
}