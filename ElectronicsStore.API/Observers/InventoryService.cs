using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;
using System;

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
            Console.WriteLine("INVENTORY OBSERVER RUNNING");
            Console.WriteLine("===== OBSERVER PATTERN =====");
            Console.WriteLine("InventoryService received OrderCreated event");
            if (donHang.ChiTietDonHangs == null || !donHang.ChiTietDonHangs.Any())
            {
                Console.WriteLine("Order has no items → skip inventory update.");
                return;
            }

            foreach (var item in donHang.ChiTietDonHangs)
            {
                var sanPham = await _context.SanPhams.FindAsync(item.MaSP);

                if (sanPham == null)
                {
                    Console.WriteLine($"Product {item.MaSP} not found.");
                    continue;
                }

                Console.WriteLine($"Checking stock for product: {sanPham.TenSP} | Current: {sanPham.SoLuongTonKho} | Ordered: {item.SoLuong}");


                if (sanPham.SoLuongTonKho < item.SoLuong)
                {
                    Console.WriteLine($"Not enough stock for {sanPham.TenSP}");
                    throw new Exception($"Sản phẩm {sanPham.TenSP} không đủ tồn kho.");
                }

                sanPham.SoLuongTonKho -= item.SoLuong;
                Console.WriteLine($"Stock updated for {sanPham.TenSP} → Remaining: {sanPham.SoLuongTonKho}");
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("Inventory updated.");
        }
    }
}