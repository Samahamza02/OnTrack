
using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface ITripStopService
    {
        Task<IEnumerable<TripStop>> GetAllAsync();

        Task<TripStop?> GetByIdAsync(int id);

        Task CreateAsync(TripStop tripStop);

        Task UpdateAsync(TripStop tripStop);

        Task DeleteAsync(int id);
    }
}
