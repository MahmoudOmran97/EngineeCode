using Microsoft.AspNetCore.Mvc;
using EngineeCode.Services;

namespace EngineeCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /api/products
        // GET /api/products?featured=true&limit=4
        // GET /api/products?category=mouse
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? featured,
            [FromQuery] string? category,
            [FromQuery] int? limit)
        {
            var products = await _productService.GetAllAsync(featured, category, limit);
            return Ok(products);
        }

        // GET /api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // ============================================================
        //  إدارة صور المنتج ✅ جديد
        // ============================================================

        // POST /api/products/5/images
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromBody] AddImageRequest req)
        {
            var image = await _productService.AddImageAsync(id, req.ImagePath, req.IsMain, req.SortOrder);
            return Ok(new { success = true, message = "تم إضافة الصورة", image });
        }

        // DELETE /api/products/5/images/3
        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var result = await _productService.DeleteImageAsync(imageId);
            if (!result) return NotFound(new { success = false, message = "الصورة غير موجودة" });
            return Ok(new { success = true, message = "تم حذف الصورة" });
        }

        // PATCH /api/products/5/images/3/set-main
        [HttpPatch("{id}/images/{imageId}/set-main")]
        public async Task<IActionResult> SetMainImage(int id, int imageId)
        {
            var result = await _productService.SetMainImageAsync(id, imageId);
            if (!result) return NotFound(new { success = false, message = "الصورة غير موجودة" });
            return Ok(new { success = true, message = "تم تعيين الصورة الرئيسية" });
        }

        // PUT /api/products/5/images/reorder
        [HttpPut("{id}/images/reorder")]
        public async Task<IActionResult> ReorderImages(int id, [FromBody] ReorderImagesRequest req)
        {
            await _productService.ReorderImagesAsync(id, req.ImageIds);
            return Ok(new { success = true, message = "تم تحديث ترتيب الصور" });
        }
    }

    // ===== Request Models =====
    public class AddImageRequest
    {
        public string ImagePath { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    public class ReorderImagesRequest
    {
        public List<int> ImageIds { get; set; } = new();
    }
}