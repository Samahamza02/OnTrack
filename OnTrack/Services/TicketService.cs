
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services
{
    public class TicketService : ITicketService
    {
        private readonly IGenericRepository<Ticket> _repository;

        public TicketService(
            IGenericRepository<Ticket> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Ticket ticket)
        {
            await _repository.AddAsync(ticket);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            var existingTicket =
                await _repository.GetByIdAsync(ticket.TicketId);

            if (existingTicket == null)
                return false;

            _repository.Update(ticket);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _repository.GetByIdAsync(id);

            if (ticket == null)
                return false;

            _repository.Remove(ticket);

            return await _repository.SaveChangesAsync() > 0;
        }
    }
}

