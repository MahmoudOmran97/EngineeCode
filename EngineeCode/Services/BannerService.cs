using EngineeCode.Data;
using EngineeCode.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineeCode.Services
{
    public class BannerService : IBannerService
    {
        private readonly AppDbContext _db;

        public BannerService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Banner>> GetActiveAsync()
        {
            var now = DateTime.Now;

            return await _db.Banners
                .Where(b => b.IsActive)
                .Where(b => b.StartDate == null || b.StartDate <= now)
                .Where(b => b.EndDate == null || b.EndDate >= now)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetAllAsync()
        {
            return await _db.Banners
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<Banner?> GetByIdAsync(int id)
        {
            return await _db.Banners.FindAsync(id);
        }

        public async Task<Banner> CreateAsync(Banner banner)
        {
            // لو محددش ترتيب، حطه فى الآخر
            if (banner.SortOrder == 0)
            {
                var maxOrder = await _db.Banners.AnyAsync()
                    ? await _db.Banners.MaxAsync(b => b.SortOrder)
                    : 0;
                banner.SortOrder = maxOrder + 1;
            }

            _db.Banners.Add(banner);
            await _db.SaveChangesAsync();
            return banner;
        }

        public async Task<Banner?> UpdateAsync(int id, Banner banner)
        {
            var existing = await _db.Banners.FindAsync(id);
            if (existing == null) return null;

            existing.Title = banner.Title;
            existing.Description = banner.Description;
            existing.BadgeText = banner.BadgeText;
            existing.CtaText = banner.CtaText;
            existing.ImagePath = banner.ImagePath;
            existing.LinkType = banner.LinkType;
            existing.TargetId = banner.TargetId;
            existing.TargetSlug = banner.TargetSlug;
            existing.ExternalUrl = banner.ExternalUrl;
            existing.SortOrder = banner.SortOrder;
            existing.IsActive = banner.IsActive;
            existing.StartDate = banner.StartDate;
            existing.EndDate = banner.EndDate;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner == null) return false;

            _db.Banners.Remove(banner);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner == null) return false;

            banner.IsActive = !banner.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderAsync(List<int> bannerIds)
        {
            var banners = await _db.Banners.ToListAsync();

            for (int i = 0; i < bannerIds.Count; i++)
            {
                var b = banners.FirstOrDefault(x => x.Id == bannerIds[i]);
                if (b != null) b.SortOrder = i;
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
