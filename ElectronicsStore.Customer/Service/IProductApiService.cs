using ElectronicsStore.Customer.Models;

namespace ElectronicsStore.Customer.Services
{
    public interface IProductApiService
    {
        Task<List<ProductViewModel>> GetAllProductsAsync();
        Task<List<CategoryViewModel>> GetCategoriesAsync(); // Để hiện Sidebar không bị trắng
        Task<ProductViewModel> GetProductByIdAsync(int id);
    }
}