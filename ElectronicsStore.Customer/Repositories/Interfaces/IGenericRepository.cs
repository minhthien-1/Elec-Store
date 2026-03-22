using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Supabase.Postgrest;

namespace ElectronicsStore.Customer.Repositories.Interfaces
{
    // T phải là một class kế thừa từ BaseModel của Supabase (Postgrest)
    public interface IGenericRepository<T> where T : BaseModel, new()
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id); // id có thể là int hoặc string
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
    }
}