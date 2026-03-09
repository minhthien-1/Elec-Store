using ElectronicsStore.API.Data.Interfaces;
using ElectronicsStore.API.Models;
using Supabase;

namespace ElectronicsStore.API.Data.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(Client client) : base(client) { }

        // Bạn không cần viết lại GetAllAsync, AddAsync, UpdateAsync 
        // vì BaseRepository đã có virtual/abstract thực hiện rồi. 
        // Trừ khi bạn muốn thay đổi logic đặc biệt cho Product.

        public override async Task<Product?> GetByIdAsync(int id)
        {
            // SỬA TẠI ĐÂY: .Single() trả về trực tiếp đối tượng Product hoặc null
            var response = await _client
                .From<Product>()
                .Where(x => x.Id == id)
                .Single();
            
            return response; // Không dùng .Model ở đây nữa
        }

        public override async Task DeleteAsync(int id)
        {
            await _client
                .From<Product>()
                .Where(x => x.Id == id)
                .Delete();
        }

        // Custom methods
        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            var response = await _client
                .From<Product>()
                .Where(x => x.Category == category)
                .Get();
                
            return response.Models ?? new List<Product>();
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
        {
            var response = await _client
                .From<Product>()
                .Where(x => x.Stock <= threshold)
                .Get();
                
            return response.Models ?? new List<Product>();
        }
    }
}