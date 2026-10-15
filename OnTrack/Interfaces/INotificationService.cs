
using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllAsync();

        Task<Notification?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Notification notification);

        Task<bool> UpdateAsync(Notification notification);

        Task<bool> DeleteAsync(int id);
    }
}

