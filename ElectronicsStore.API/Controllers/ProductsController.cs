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
            var products = await _productRepository.GetAllAsync();
            // Mapping sang anonymous object để đồng bộ camelCase với view
            var result = products.Select(p => new {
                maSP = p.MaSP,
                tenSP = p.TenSP,
                giaBan = p.GiaBan,
                soLuongTonKho = p.SoLuongTonKho,
                maDanhMuc = p.MaDanhMuc,
                hinhAnh = p.HinhAnh
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Lỗi khi lấy danh sách", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _productRepository.GetByIdAsync(id);
        if (p == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

        // Trả về cùng cấu trúc với GetAll để frontend không bị lỗi "undefined"
        return Ok(new {
            maSP = p.MaSP,
            tenSP = p.TenSP,
            giaBan = p.GiaBan,
            soLuongTonKho = p.SoLuongTonKho,
            maDanhMuc = p.MaDanhMuc,
            hinhAnh = p.HinhAnh
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var result = await _productRepository.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = result.MaSP }, result);
    }

    // Bổ sung hàm Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.MaSP) return BadRequest(new { message = "ID không khớp" });
        
        await _productRepository.UpdateAsync(product);
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();

        await _productRepository.DeleteAsync(id);
        return NoContent();
    }
}
}