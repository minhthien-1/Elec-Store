using ElectronicsStore.API.Models; // Share model hoặc tạo model tương đương
using System.Net.Http.Json;

public class ProductApiService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "http://localhost:5145/api/products"; // Cổng API của bạn

    public ProductApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        // Gọi đến API mà bạn vừa làm ở trên
        return await _httpClient.GetFromJsonAsync<List<Product>>(ApiUrl) ?? new List<Product>();
    }
}