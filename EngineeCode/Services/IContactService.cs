using EngineeCode.Models;

namespace EngineeCode.Services
{
    public interface IContactService
    {
        Task SaveMessageAsync(ContactMessage message);
    }
}
