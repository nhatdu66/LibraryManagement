using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagementSystem.Repositories
{
    public class SeriesRepository : GenericRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Series>> GetAllSeriesAsync()
        {
            return await _dbSet.OrderBy(s => s.SeriesName).ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
