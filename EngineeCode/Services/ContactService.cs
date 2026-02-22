using EngineeCode.Data;
using EngineeCode.Models;
using System;

namespace EngineeCode.Services
{
    public class ContactService : IContactService
    {
        private readonly AppDbContext _db;

        public ContactService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SaveMessageAsync(ContactMessage message)
        {
            _db.ContactMessages.Add(message);
            await _db.SaveChangesAsync();
        }
    }
}
