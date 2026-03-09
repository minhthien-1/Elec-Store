using ElectronicsStore.API.Data.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;

namespace ElectronicsStore.API.Data.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseModel, new()
    {
        protected readonly Client _client;

        protected BaseRepository(Client client)
        {
            _client = client;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            Console.WriteLine($"[REPOSITORY] Đang truy cập tầng dữ liệu để lấy danh sách {typeof(T).Name}...");
            var response = await _client.From<T>().Get();
            return response.Models ?? new List<T>();
        }

        // Bổ sung phương thức GetByIdAsync bị thiếu
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            // Với Supabase, ta lọc theo cột 'id' (giả định cột PK của bạn tên là id)
            var response = await _client.From<T>()
                                       .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                                       .Single();
            return response;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            var response = await _client.From<T>().Insert(entity);
            return response.Models.FirstOrDefault() ?? entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            await _client.From<T>().Update(entity);
        }

        // Bổ sung phương thức DeleteAsync bị thiếu
        public virtual async Task DeleteAsync(int id)
        {
            // Lọc đúng bản ghi có id tương ứng và xóa
            await _client.From<T>()
                         .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                         .Delete();
        }
    }
}