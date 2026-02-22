using EngineeCode.Data;
using EngineeCode.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace EngineeCode.Services
{
    public class ServiceService : IServiceService
    {
        private readonly AppDbContext _db;

        public ServiceService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Service>> GetAllAsync(int? limit = null)
        {
            var query = _db.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }
    }
}
