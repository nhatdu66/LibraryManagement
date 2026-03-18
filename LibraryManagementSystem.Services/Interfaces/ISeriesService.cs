using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface ISeriesService
    {
        Task<IEnumerable<Series>> GetSeriesAsync();
        Task AddSeriesAsync(Series series);
    }
}
