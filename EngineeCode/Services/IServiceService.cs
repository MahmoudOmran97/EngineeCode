using EngineeCode.Models;

namespace EngineeCode.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllAsync(int? limit = null);
    }
}
