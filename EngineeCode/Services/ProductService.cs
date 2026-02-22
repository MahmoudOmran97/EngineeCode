using EngineeCode.Data;
using EngineeCode.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineeCode.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;

        public ProductService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(
            bool? featured = null,
            string? category = null,
            int? limit = null)
        {
            var query = _db.Products
                .Where(p => p.IsActive)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .AsQueryable();

            if (featured.HasValue)
                query = query.Where(p => p.IsFeatured == featured.Value);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            query = query.OrderByDescending(p => p.SalesCount);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductImage> AddImageAsync(int productId, string imagePath, bool isMain = false, int sortOrder = 0)
        {
            if (isMain)
            {
                var oldImages = await _db.ProductImages
                    .Where(i => i.ProductId == productId && i.IsMain)
                    .ToListAsync();
                oldImages.ForEach(i => i.IsMain = false);

                var product = await _db.Products.FindAsync(productId);
                if (product != null) product.ImagePath = imagePath;
            }

            var image = new ProductImage
            {
                ProductId = productId,
                ImagePath = imagePath,
                IsMain = isMain,
                SortOrder = sortOrder
            };

            _db.ProductImages.Add(image);
            await _db.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            var image = await _db.ProductImages.FindAsync(imageId);
            if (image == null) return false;

            _db.ProductImages.Remove(image);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetMainImageAsync(int productId, int imageId)
        {
            var images = await _db.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            var target = images.FirstOrDefault(i => i.Id == imageId);
            if (target == null) return false;

            images.ForEach(i => i.IsMain = (i.Id == imageId));

            var product = await _db.Products.FindAsync(productId);
            if (product != null) product.ImagePath = target.ImagePath;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderImagesAsync(int productId, List<int> imageIds)
        {
            var images = await _db.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            for (int i = 0; i < imageIds.Count; i++)
            {
                var img = images.FirstOrDefault(x => x.Id == imageIds[i]);
                if (img != null) img.SortOrder = i;
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }
}