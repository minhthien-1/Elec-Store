using ElectronicsStore.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ElectronicsStore.Customer.Decorator;
using ElectronicsStore.AbstractFactory;
using ElectronicsStore.AbstractFactory.Factories;
using ElectronicsStore.Customer.Services; 
using ElectronicsStore.Customer.Service;
using ElectronicsStore.Customer.Service.Payment;
using ElectronicsStore.Customer.Repositories;
using ElectronicsStore.Customer.Repositories.Interfaces;
using ElectronicsStore.API.Commands;
using ElectronicsStore.API.Observers;
using ElectronicsStore.Customer.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using ElectronicsStore.Customer.Patterns;
using ElectronicsStore.Customer.Service.Proxy;
using ElectronicsStore.Customer.Service.Adapter;


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
            builder.Services.AddServerSideBlazor();
            builder.Services.AddHttpClient();

            // Đăng ký Generic Repository
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            
            // Đăng ký ProductBuilder vào DI Container
            builder.Services.AddScoped<IProductBuilder, ProductBuilder>();

            // Đăng ký Login Patterns
builder.Services.AddScoped<LocalLoginStrategy>();
builder.Services.AddScoped<ExternalLoginStrategy>();
builder.Services.AddScoped<LoginStrategyFactory>();

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

            // --- 3. CẤU HÌNH ĐĂNG NHẬP (COOKIE, GOOGLE & FACEBOOK) ---
            builder.Services.AddAuthentication(options =>
            {
                // Đặt Cookie làm phương thức xác thực mặc định
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Home/Index";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Home/Index";
                options.Cookie.Name = "ElectronicsStore_Session";
            })
            .AddCookie("ExternalCookie") // Tạo Cookie tạm để hứng dữ liệu
            .AddGoogle(googleOptions =>
            {
                googleOptions.SignInScheme = "ExternalCookie";
                
                IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
                googleOptions.ClientId = googleAuthNSection["ClientId"];
                googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.SignInScheme = "ExternalCookie";
                
                IConfigurationSection fbAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
                facebookOptions.AppId = fbAuthNSection["AppId"];
                facebookOptions.AppSecret = fbAuthNSection["AppSecret"];
            });

            // --- ĐĂNG KÝ DESIGN PATTERNS MỚI ---

            // Proxy Pattern: Đăng ký Service Báo cáo
            builder.Services.AddScoped<IReportService, AdminReportProxy>(provider => 
                new AdminReportProxy("Staff")); // Mặc định thử nghiệm với quyền Staff

            // Adapter Pattern: Đăng ký Service Thông báo
            builder.Services.AddScoped<OldEmailSystem>();
            builder.Services.AddScoped<INotificationService, EmailAdapter>();

            // Sau khi khai báo hết Services mới gọi Build
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("       DEMO DESIGN PATTERNS BỔ SUNG");
                Console.WriteLine(new string('=', 50));

                // 1. Test Proxy Pattern
                Console.WriteLine("\n[PROXY PATTERN TEST]");
                // Thử với quyền Staff (đã đăng ký ở trên)
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                reportService.DisplayReport();
                
                // Thử tạo trực tiếp với quyền Admin để thấy sự khác biệt
                Console.WriteLine("-> Thử lại với quyền Admin:");
                var adminReport = new AdminReportProxy("Admin");
                adminReport.DisplayReport();

                // 2. Test Adapter Pattern
                Console.WriteLine("\n[ADAPTER PATTERN TEST]");
                var notification = scope.ServiceProvider.GetRequiredService<INotificationService>();
                notification.Send("Đơn hàng điện tử mới đã được tạo thành công!");

                Console.WriteLine(new string('=', 50) + "\n");
            }
            // --- 4. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            
            // Đảm bảo Authentication luôn đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapBlazorHub();

            // Dòng này phải là dòng CUỐI CÙNG của Main
            app.Run();
        }
    }
}