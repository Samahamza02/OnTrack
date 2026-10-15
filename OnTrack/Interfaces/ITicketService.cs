
using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllAsync();

        Task<Ticket?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Ticket ticket);

        Task<bool> UpdateAsync(Ticket ticket);

        Task<bool> DeleteAsync(int id);
    }
}

