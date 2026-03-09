using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.API.Observers
{
    public class InventoryService : IOrderObserver
    {
        private readonly ElectronicsStoreDbContext _context;

        public InventoryService(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        public async Task OnOrderCreated(DonHang donHang)
        {
            if (donHang.ChiTietDonHangs == null || !donHang.ChiTietDonHangs.Any())
                return;

            foreach (var item in donHang.ChiTietDonHangs)
            {
                var sanPham = await _context.SanPhams.FindAsync(item.MaSP);

                if (sanPham == null)
                    continue;

                if (sanPham.SoLuongTonKho < item.SoLuong)
                {
                    throw new Exception($"Sản phẩm {sanPham.TenSP} không đủ tồn kho.");
                }

                sanPham.SoLuongTonKho -= item.SoLuong;
            }

            await _context.SaveChangesAsync();
        }
    }
}