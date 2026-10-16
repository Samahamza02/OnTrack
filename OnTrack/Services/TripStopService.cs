
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services
{
    public class TripStopService : ITripStopService
    {
        private readonly IGenericRepository<TripStop> _repository;

        public TripStopService(
            IGenericRepository<TripStop> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TripStop>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TripStop?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task CreateAsync(TripStop tripStop)
        {
            await _repository.AddAsync(tripStop);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripStop tripStop)
        {
            _repository.Update(tripStop);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tripStop = await _repository.GetByIdAsync(id);

            if (tripStop != null)
            {
                _repository.Remove(tripStop);
                await _repository.SaveChangesAsync();
            }
        }
    }
}
