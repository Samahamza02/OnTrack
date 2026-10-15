
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IGenericRepository<Payment> _repository;

        public PaymentService(
            IGenericRepository<Payment> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Payment payment)
        {
            await _repository.AddAsync(payment);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            var existingPayment =
                await _repository.GetByIdAsync(payment.PaymentId);

            if (existingPayment == null)
                return false;

            _repository.Update(payment);

            return await _repository.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _repository.GetByIdAsync(id);

            if (payment == null)
                return false;

            _repository.Remove(payment);

            return await _repository.SaveChangesAsync() > 0;
        }
    }
}
