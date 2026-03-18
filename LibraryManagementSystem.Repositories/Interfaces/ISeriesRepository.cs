using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface ISeriesRepository : IGenericRepository<Series>
    {
        Task<IEnumerable<Series>> GetAllSeriesAsync();
        Task<int> SaveChangesAsync();
    }
}
