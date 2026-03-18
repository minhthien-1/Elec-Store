using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ElectronicsStore.Customer.Decorator;
using ElectronicsStore.AbstractFactory;
using ElectronicsStore.AbstractFactory.Factories;
using ElectronicsStore.Customer.Services; 
using ElectronicsStore.Customer.Service;// Đảm bảo có using này
using ElectronicsStore.Customer.Service.Payment;
using ElectronicsStore.API.Commands;
using ElectronicsStore.API.Observers;

namespace ElectronicsStore.Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // --- 1. CẤU HÌNH DATABASE ---
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ElectronicsStoreDbContext>(options =>
                options.UseNpgsql(connectionString));

            // --- 2. CẤU HÌNH SERVICES & HTTPCLIENT ---
            builder.Services.AddControllersWithViews();

           // Đăng ký Service với cấu hình chuẩn
    // Đăng ký Service theo cách tường minh nhất
    builder.Services.AddHttpClient<IProductApiService, ProductApiService>()
    .ConfigureHttpClient(client => 
    {
        client.BaseAddress = new Uri("http://localhost:5145/api/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => 
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            AllowAutoRedirect = false 
        };
    });
            builder.Services.AddHttpClient<OrderService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7206/");
            });
            builder.Services.AddScoped<OrderSubject>();
            builder.Services.AddScoped<CreateOrderCommand>();
            builder.Services.AddScoped<ElectronicsStore.Customer.Service.Pricing.PricingStrategyFactory>();
            builder.Services.AddScoped<PaymentFactory>();
            // --- 3. CẤU HÌNH ĐĂNG NHẬP ---
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Index";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Home/Index";
                    options.Cookie.Name = "ElectronicsStore_Session";
                });

            // Sau khi khai báo hết Services mới gọi Build
            var app = builder.Build();

            // --- 4. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
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

            // Dòng này phải là dòng CUỐI CÙNG của Main
            app.Run();
        }
    }
}