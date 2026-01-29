using ElectronicsStore.API.Data;

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace ElectronicsStore.Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. CẤU HÌNH DATABASE (SUPABASE) ---
            // Lấy chuỗi kết nối "DefaultConnection" từ file appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Đăng ký DbContext sử dụng Npgsql (PostgreSQL)
            builder.Services.AddDbContext<ElectronicsStoreDbContext>(options =>
                options.UseNpgsql(connectionString));

            // --- 2. CẤU HÌNH SERVICES ---
            builder.Services.AddControllersWithViews();

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

            app.UseAuthorization();

            // Cấu hình Route mặc định để chạy vào HomeController -> Index
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}