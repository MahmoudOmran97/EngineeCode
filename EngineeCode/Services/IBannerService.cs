using EngineeCode.Models;

namespace EngineeCode.Services
{
    public interface IBannerService
    {
        // للموقع العام — البنرات الشغالة بس، بالترتيب، ومراعية الجدولة (Start/End Date)
        Task<IEnumerable<Banner>> GetActiveAsync();

        // للإدارة — كل البنرات مهما كانت حالتها
        Task<IEnumerable<Banner>> GetAllAsync();
        Task<Banner?> GetByIdAsync(int id);

        Task<Banner> CreateAsync(Banner banner);
        Task<Banner?> UpdateAsync(int id, Banner banner);
        Task<bool> DeleteAsync(int id);

        Task<bool> ToggleActiveAsync(int id);
        Task<bool> ReorderAsync(List<int> bannerIds);
    }
}
