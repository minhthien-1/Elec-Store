using ElectronicsStore.Customer.Models; 
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
            try 
            {
                return await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("products") ?? new List<ProductViewModel>();
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gọi API Products: " + ex.Message);
                return new List<ProductViewModel>();
            }
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            try 
            {
                // 1. Dùng CategoryApiResponse (Cái Hộp) để hứng JSON
                var response = await _httpClient.GetFromJsonAsync<CategoryApiResponse>("category");
                
                // 2. Bóc lấy phần danh sách bên trong chữ 'data'
                if (response != null && response.data != null) 
                {
                    return response.data;
                }
                
                return new List<CategoryViewModel>();
            } 
            catch (Exception ex) 
            {
                // In ra để biết nếu nó còn lỗi thì nó chửi câu gì
                Console.WriteLine("Lỗi bóc JSON Category: " + ex.Message);
                return new List<CategoryViewModel>();
            }
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            try 
            {
                return await _httpClient.GetFromJsonAsync<ProductViewModel>($"products/{id}");
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API Product Details ({id}): " + ex.Message);
                return null!;
            }
        }
    }
}