using ElectronicsStore.Customer.Models.ViewModels;

namespace ElectronicsStore.Customer.Services
{
    public class ProductApiService
    {
        private readonly HttpClient _httpClient;

        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Địa chỉ API của bạn (Ví dụ: https://localhost:7000)
            // Nhớ kiểm tra port của project API trong launchSettings.json
            _httpClient.BaseAddress = new Uri("https://localhost:7000");
        }

        public async Task<ProductDetailViewModel?> GetProductByIdAsync(int id)
        {
            try
            {
                // Gọi vào endpoint GET: /api/product/{id}
                var response = await _httpClient.GetAsync($"/api/product/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductDetailViewModel>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}