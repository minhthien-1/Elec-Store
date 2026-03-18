using System.Net.Http;
using System.Net.Http.Json;
using static ElectronicsStore.API.Controllers.OrderController;
using ElectronicsStore.Customer.Models;

public class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "https://localhost:7206/api/order/create",
            request
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();

        return result.orderId;
    }
}