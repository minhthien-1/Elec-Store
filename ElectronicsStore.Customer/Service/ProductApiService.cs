using ElectronicsStore.Customer.Models; // Sử dụng Model của Customer
using System.Net.Http.Json;

namespace ElectronicsStore.Customer.Services
{
    public class ProductApiService : IProductApiService 
    {
        private readonly HttpClient _httpClient;

        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            try {
                // Không cần ghi full localhost vì đã cấu hình BaseAddress ở Program.cs rồi
                return await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products") ?? new List<ProductViewModel>();
            } catch {
                return new List<ProductViewModel>();
            }
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            try {
                return await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("categories") ?? new List<CategoryViewModel>();
            } catch {
                return new List<CategoryViewModel>();
            }
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            try {
                return await _httpClient.GetFromJsonAsync<ProductViewModel>($"products/{id}");
            } catch {
                return null;
            }
        }
    }
}