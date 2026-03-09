using System.Text;
using ElectronicsStore.API.Data;
using ElectronicsStore.Customer.Service.Payment;
using ElectronicsStore.Customer.Service.Pricing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;



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

            // --- CẤU HÌNH ĐĂNG NHẬP (BỎ EXPIRETIMESPAN ĐỂ DÙNG SESSION COOKIE) ---
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Index";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Home/Index";
                    options.Cookie.Name = "ElectronicsStore_Session";
                    // Không đặt ExpireTimeSpan ở đây để ưu tiên Session Cookie của trình duyệt
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

            // Thứ tự quan trọng: Authentication trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}