using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ElectronicsStore.Customer.Decorator;
using ElectronicsStore.AbstractFactory;
using ElectronicsStore.AbstractFactory.Factories;
using ElectronicsStore.Customer.Services; // Đảm bảo có using này

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

            builder.Services.AddHttpClient("Default", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5145");
            });
            // QUAN TRỌNG: Đăng ký ProductApiService VÀ xử lý SSL cùng lúc tại đây
            builder.Services.AddHttpClient<ProductApiService>(client => 
            {
                client.BaseAddress = new Uri("http://localhost:5145");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // 1. Chấp nhận mọi chứng chỉ (nếu lỡ bị nhảy sang https)
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                
                // 2. CẤM TỰ ĐỘNG CHUYỂN HƯỚNG (Ngăn việc bị bắt sang https)
                AllowAutoRedirect = false 
            });

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