using ElectronicsStore.API.Data;
using ElectronicsStore.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ElectronicsStore.API.Observers;
using ElectronicsStore.API.Commands;
// === NEW: Thêm các namespace cần thiết ===
using ElectronicsStore.API.Data.Interfaces;
using ElectronicsStore.API.Data.Repositories;

namespace ElectronicsStore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<CreateOrderCommand>();

            // Add services to the container
            builder.Services.AddScoped<OrderSubject>();
            builder.Services.AddScoped<IOrderObserver, EmailNotifier>();
            builder.Services.AddScoped<IOrderObserver, InventoryService>();
            builder.Services.AddScoped<IOrderObserver, TerminalLoggerObserver>();
            builder.Services.AddScoped<IOrderObserver, InventoryService>();
            builder.Services.AddScoped<IOrderObserver, EmailNotifier>();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // 1. Bỏ qua các vòng lặp tham chiếu (nếu có)
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    
                    // 2. QUAN TRỌNG: Không cố gắng serialize các thuộc tính phức tạp của Supabase BaseModel
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    
                    // 3. Giúp Swagger hiển thị đẹp hơn
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // === NEW: Cấu hình Supabase Client (Singleton Pattern) ===
            // Lấy thông tin từ appsettings.json
            var supabaseUrl = builder.Configuration["Supabase:Url"];
            var supabaseKey = builder.Configuration["Supabase:Key"];
            
            // Đăng ký Supabase Client là Singleton vì chúng ta chỉ cần 1 instance dùng chung
            builder.Services.AddSingleton(provider =>
            {
                // Dòng này sẽ hiện ra Terminal để chứng minh Singleton
                Console.WriteLine("[SINGLETON] Supabase Client đang được khởi tạo");
                
                var url = builder.Configuration["Supabase:Url"];
                var key = builder.Configuration["Supabase:Key"];
                Console.WriteLine($"DEBUG: Url lấy được là: '{url}'");
                Console.WriteLine($"DEBUG: Key lấy được là: '{(string.IsNullOrEmpty(key) ? "NULL" : "CÓ DỮ LIỆU")}'");
                return new Supabase.Client(supabaseUrl!, supabaseKey!, new Supabase.SupabaseOptions { AutoConnectRealtime = true });
            });
                

            // === NEW: Đăng ký Repository (Repository Pattern) ===
            // Đăng ký ProductRepository dưới dạng Scoped
            builder.Services.AddScoped<IProductRepository, ProductRepository>();


            // ADD DATABASE (PostgreSQL - Neon)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            try 
            {
                builder.Services.AddDbContext<ElectronicsStoreDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            }
            catch (Exception) 
            {
                Console.WriteLine(">>> [Hệ thống] Tạm thời bỏ qua lỗi kết nối EF Core để Demo Pattern...");
            }

            // ADD JWT AUTHENTICATION
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddDebug();
            });
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}