using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: /Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();

            return View(payments);
        }

        // GET: /Payments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
    }
}
