
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notification> _repository;

        public NotificationService(
            IGenericRepository<Notification> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(
            Notification notification)
        {
            await _repository.AddAsync(notification);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(
            Notification notification)
        {
            var existingNotification =
                await _repository.GetByIdAsync(
                    notification.NotificationId);

            if (existingNotification == null)
                return false;

            _repository.Update(notification);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification =
                await _repository.GetByIdAsync(id);

            if (notification == null)
                return false;

            _repository.Remove(notification);

            return await _repository.SaveChangesAsync() > 0;
        }
    }
}

