using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface IStationService
    {
        Task<List<Station>> GetAllAsync(string? search = null);
        Task<Station?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string name, string city, int? excludeId = null);
        Task<bool> IsInUseAsync(int id);
        Task<(int stops, int tickets)> GetUsageAsync(int id);
        Task CreateAsync(Station station);
        Task UpdateAsync(Station station);
        Task DeleteAsync(int id);
    }
}