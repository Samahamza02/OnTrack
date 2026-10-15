
using OnTrack.Models;

namespace OnTrack.Interfaces
{
    public interface IEvaluationService
    {
        Task<IEnumerable<Evaluation>> GetAllAsync();

        Task<Evaluation?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Evaluation evaluation);

        Task<bool> UpdateAsync(Evaluation evaluation);

        Task<bool> DeleteAsync(int id);
    }
}

