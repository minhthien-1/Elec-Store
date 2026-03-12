using ElectronicsStore.API.Data.Interfaces;
using ElectronicsStore.API.Models;
using Supabase;

namespace ElectronicsStore.API.Data.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(Client client) : base(client) { }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Đã xóa .Select() vì [Reference] trong Model đã tự động lấy data hãng rồi
            var response = await _client
                .From<Product>()
                .Get();
            
            return response.Models ?? new List<Product>();
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            // Đã xóa .Select() ở đây luôn
            var response = await _client
                .From<Product>()
                .Where(x => x.MaSP == id)
                .Single();
            
            return response;
        }

        public override async Task DeleteAsync(int id)
        {
            await _client
                .From<Product>()
                .Where(x => x.MaSP == id)
                .Delete();
        }

        // Custom methods
        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            var response = await _client
                .From<Product>()
                .Where(x => x.MaDanhMuc == int.Parse(category))
                .Get();
                
            return response.Models ?? new List<Product>();
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
        {
            var response = await _client
                .From<Product>()
                .Where(x => x.SoLuongTonKho <= threshold)
                .Get();
                
            return response.Models ?? new List<Product>();
        }
    }
}