using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.API.Data;
using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly ElectronicsStoreDbContext _context;

        public ProductController(ElectronicsStoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.SanPhams
                                         .Include(s => s.DanhMuc)
                                         .OrderByDescending(s => s.MaSP)
                                         .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.DanhMucSanPhams.Where(c => c.TrangThai).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SanPham product)
        {
            if (ModelState.IsValid)
            {
                product.ThemTrongDB = DateTime.UtcNow;
                _context.SanPhams.Add(product);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.Categories = await _context.DanhMucSanPhams.Where(c => c.TrangThai).ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.SanPhams.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.DanhMucSanPhams.Where(c => c.TrangThai).ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SanPham product)
        {
            var existingProduct = await _context.SanPhams.FindAsync(product.MaSP);
            if (existingProduct == null) return NotFound();

            existingProduct.TenSP = product.TenSP;
            existingProduct.GiaBan = product.GiaBan;
            existingProduct.SoLuongTonKho = product.SoLuongTonKho;
            existingProduct.MaDanhMuc = product.MaDanhMuc;
            existingProduct.HinhAnh = product.HinhAnh;
            existingProduct.MoTaChiTiet = product.MoTaChiTiet;
            existingProduct.SuaDoi = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var product = await _context.SanPhams.FindAsync(request.Id);
            if (product != null)
            {
                _context.SanPhams.Remove(product);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
        }
    }

    public class DeleteRequest
    {
        public int Id { get; set; }
    }
}