using EngineeCode.Models;

namespace EngineeCode.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync(bool? featured = null, string? category = null, int? limit = null);
        Task<Product?> GetByIdAsync(int id);

        // ===== إدارة الصور ===== ✅ جديد
        Task<ProductImage> AddImageAsync(int productId, string imagePath, bool isMain = false, int sortOrder = 0);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<bool> ReorderImagesAsync(int productId, List<int> imageIds);
    }
}