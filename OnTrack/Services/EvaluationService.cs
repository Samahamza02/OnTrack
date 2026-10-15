
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IGenericRepository<Evaluation> _repository;

        public EvaluationService(
            IGenericRepository<Evaluation> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Evaluation>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Evaluation?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Evaluation evaluation)
        {
            if (evaluation.rating < 1 || evaluation.rating > 5)
                return false;

            await _repository.AddAsync(evaluation);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Evaluation evaluation)
        {
            if (evaluation.rating < 1 || evaluation.rating > 5)
                return false;

            var existingEvaluation =
                await _repository.GetByIdAsync(
                    evaluation.EvaluationId);

            if (existingEvaluation == null)
                return false;

            _repository.Update(evaluation);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evaluation =
                await _repository.GetByIdAsync(id);

            if (evaluation == null)
                return false;

            _repository.Remove(evaluation);

            return await _repository.SaveChangesAsync() > 0;
        }
    }
}

