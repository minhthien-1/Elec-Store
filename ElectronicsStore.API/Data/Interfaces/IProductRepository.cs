using ElectronicsStore.API.Models;

namespace ElectronicsStore.API.Data.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10);
    }
}
