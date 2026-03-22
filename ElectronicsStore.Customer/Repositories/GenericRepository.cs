using ElectronicsStore.Customer.Repositories.Interfaces;
using ElectronicsStore.Customer.Service.Singleton;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Supabase.Postgrest;

namespace ElectronicsStore.Customer.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseModel, new()
    {
        private readonly ILogger<GenericRepository<T>> _logger;
        protected readonly Supabase.Client _supabase;

        public GenericRepository(ILogger<GenericRepository<T>> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Lấy cấu hình và khởi tạo Supabase thông qua Singleton
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];
            
            // Đảm bảo url và key không null trước khi gọi Singleton
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
            {
                _logger.LogError("Supabase URL hoặc Key chưa được cấu hình trong appsettings.json");
            }

            _supabase = SupabaseProvider.GetInstance(url!, key!);
        }

        public async Task<List<T?>> GetAllAsync()
        {
            var response = await _supabase.From<T>().Get();
            return response.Models;
        }

        public async Task<T> GetByIdAsync(object id)
        {
        _logger.LogInformation("\n========== [REPO + SINGLETON PATTERN] ==========");
        _logger.LogInformation($"[SINGLETON] Kết nối Supabase Instance ID: {_supabase.GetHashCode()}");
        _logger.LogInformation($"[REPOSITORY] Đang truy vấn bảng [{typeof(T).Name}] cho ID: {id}");
        _logger.LogInformation("==============================================\n");
            // Giả định cột ID của bạn luôn là 'MaND', 'MaSP'... 
            // Supabase/Postgrest cần chỉ định rõ cột nếu tên không phải là 'id'
            var response = await _supabase.From<T>().Get();
            return response.Models.FirstOrDefault(); 
            // Lưu ý: Tùy vào bảng mà bạn có thể tùy biến hàm GetById này
        }

        public async Task<bool> InsertAsync(T entity)
        {
            var response = await _supabase.From<T>().Insert(entity);
            return response.ResponseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var response = await _supabase.From<T>().Update(entity);
            return response.ResponseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            await _supabase.From<T>().Delete(entity);
            return true;
        }
    }
}