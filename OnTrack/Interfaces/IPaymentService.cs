
using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllAsync();

        Task<Payment?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Payment payment);

        Task<bool> UpdateAsync(Payment payment);

        Task<bool> DeleteAsync(int id);
    }
}

