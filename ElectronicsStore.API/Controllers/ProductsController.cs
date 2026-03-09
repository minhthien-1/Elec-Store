using ElectronicsStore.API.Data.Interfaces;
using ElectronicsStore.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
     try 
    {
        // 1. Gọi qua Repository (Để hiện chữ [REPOSITORY] trong Terminal)
        var products = await _productRepository.GetAllAsync();

        // 2. Chắt lọc dữ liệu (Để tránh lỗi JSON cồng kềnh của Supabase)
        var result = products.Select(p => new {
            maSP = p.Id,
            tenSP = p.Name,
            giaBan = p.Price,
            soLuongTonKho = p.Stock,
            danhMuc = p.Category,
            hinhAnh = p.HinhAnh,
            // Bạn có thể thêm các trường khác nếu Model Product của bạn có nhé
        });

        return Ok(result);
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = "Lỗi hệ thống", details = ex.Message });
    }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product) // Thêm [FromBody] để Swagger bắt dữ liệu tốt hơn
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}