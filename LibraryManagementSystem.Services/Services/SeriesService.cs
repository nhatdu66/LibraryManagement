using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services.Services
{
    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;

        public SeriesService(ISeriesRepository seriesRepository)
        {
            _seriesRepository = seriesRepository;
        }

        public async Task<IEnumerable<Series>> GetSeriesAsync()
        {
            return await _seriesRepository.GetAllSeriesAsync();
        }

        public async Task AddSeriesAsync(Series series)
        {
            await _seriesRepository.AddAsync(series);
            await _seriesRepository.SaveChangesAsync();
        }
    }
}
