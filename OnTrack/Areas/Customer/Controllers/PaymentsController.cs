
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Models.ViewModels;
using System.Security.Claims;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _context;

        public PaymentsController(
            IPaymentService paymentService,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        // GET: /Customer/Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }

        // GET: /Customer/Payments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: /Customer/Payments/ChooseMethod
        [HttpGet]
        public async Task<IActionResult> ChooseMethod(
            int tripId,
            int[] ticketIds)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (ticketIds.Length == 0)
                return BadRequest("No tickets were selected.");

            var tickets = await _context.Tickets
                .Where(t =>
                    ticketIds.Contains(t.TicketId) &&
                    t.TripId == tripId &&
                    t.UserId == userId &&
                    t.Status == "PendingPayment")
                .ToListAsync();

            if (tickets.Count != ticketIds.Distinct().Count())
                return NotFound();

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return NotFound();

            var vm = new PaymentViewModel
            {
                TripId = tripId,
                RouteName = trip.RouteName,
                TicketIds = tickets.Select(t => t.TicketId).ToList(),
                TotalAmount = tickets.Sum(t => t.Price)
            };

            return View(vm);
        }

        // POST: /Customer/Payments/ChooseMethod

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseMethod(PaymentViewModel vm)
        {
            var userId = User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var ticketIds = vm.TicketIds?.Distinct().ToList()
                            ?? new List<int>();

            if (ticketIds.Count == 0)
                return BadRequest("No tickets were selected.");

            if (vm.PaymentMethod != "Cash" &&
                vm.PaymentMethod != "Card")
            {
                ModelState.AddModelError(
                    nameof(vm.PaymentMethod),
                    "Please choose Cash or Card.");
            }

            var tickets = await _context.Tickets
                .Where(t => ticketIds.Contains(t.TicketId)
                    && t.TripId == vm.TripId
                    && t.UserId == userId
                    && t.Status == "PendingPayment")
                .ToListAsync();

            if (tickets.Count != ticketIds.Count)
                return BadRequest("The selected tickets are not available for payment.");

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == vm.TripId);

            if (trip == null)
                return NotFound();

            vm.TicketIds = ticketIds;
            vm.RouteName = trip.RouteName;
            vm.TotalAmount = tickets.Sum(t => t.Price);

            if (!ModelState.IsValid)
                return View(vm);

            // CARD: create a Stripe Checkout Session.
            if (vm.PaymentMethod == "Card")
            {
                var secretKey =
                    HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()
                        ["Stripe:SecretKey"];

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    ModelState.AddModelError(
                        "",
                        "Stripe is not configured. Please contact the administrator.");

                    return View(vm);
                }

                var currency = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    ["Stripe:Currency"] ?? "egp";

                var totalAmount = tickets.Sum(t => t.Price);

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    Mode = "payment",
                   AllowedPaymentMethodTypes = new List<string>
                            {
                                "card"
                            },

                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = (long)Math.Round(
                            totalAmount * 100,
                            MidpointRounding.AwayFromZero),

                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"OnTrack Train Booking - {trip.RouteName}"
                        }
                    }
                }
            },

                    SuccessUrl = Url.Action(
                        nameof(StripeSuccess),
                        "Payments",
                        new { area = "Customer" },
                        Request.Scheme)
                        + "?session_id={CHECKOUT_SESSION_ID}",

                    CancelUrl = Url.Action(
                        nameof(StripeCancel),
                        "Payments",
                        new { area = "Customer", tripId = vm.TripId },
                        Request.Scheme),

                    ClientReferenceId = userId,

                    Metadata = new Dictionary<string, string>
                    {
                        ["UserId"] = userId,
                        ["TripId"] = vm.TripId.ToString(),
                        ["TicketIds"] = string.Join(",", ticketIds)
                    }
                };

                var service = new Stripe.Checkout.SessionService();
                var session = await service.CreateAsync(options);

                return Redirect(session.Url);
            }

            // CASH: record a pending payment.
            foreach (var ticket in tickets)
            {
                _context.Payments.Add(new Payment
                {
                    TicketId = ticket.TicketId,
                    Amount = ticket.Price,
                    PaymentMethod = "Cash",
                    Status = "Pending",
                    PaidAt = DateTime.Now,
                    TransactionRef = Guid.NewGuid().ToString("N")
                });

                ticket.Status = "Booked";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your cash payment choice was saved. Payment is still pending.";

            return RedirectToAction(
                "Index",
                "Payments",
                new { area = "Customer" });
        }


        [HttpGet]
        public async Task<IActionResult> StripeSuccess(string session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Missing Stripe session.");

            var userId = User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var service = new Stripe.Checkout.SessionService();
            var session = await service.GetAsync(session_id);

            // Never mark tickets as paid just because the browser returned.
            if (session.PaymentStatus != "paid")
            {
                TempData["Success"] =
                    "Payment has not been completed. Please try again.";

                return RedirectToAction(
                    "Index", "Payments", new { area = "Customer" });
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("UserId", out var paymentUserId) ||
                paymentUserId != userId ||
                !session.Metadata.TryGetValue("TripId", out var tripIdText) ||
                !int.TryParse(tripIdText, out var tripId) ||
                !session.Metadata.TryGetValue("TicketIds", out var idsText))
            {
                return Forbid();
            }

            var ticketIds = idsText.Split(',')
                .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ticketIds.Count == 0)
                return BadRequest("No tickets were found for this payment.");

            var tickets = await _context.Tickets
                .Where(t => ticketIds.Contains(t.TicketId)
                    && t.TripId == tripId
                    && t.UserId == userId)
                .ToListAsync();

            if (tickets.Count != ticketIds.Count)
                return BadRequest("The booking could not be verified.");

            var transactionRef = session.PaymentIntentId ?? session.Id;

            foreach (var ticket in tickets)
            {
                // Avoid creating duplicate payment records on refresh.
                var alreadyRecorded = await _context.Payments.AnyAsync(p =>
                    p.TicketId == ticket.TicketId &&
                    p.TransactionRef == transactionRef);

                if (!alreadyRecorded)
                {
                    _context.Payments.Add(new Payment
                    {
                        TicketId = ticket.TicketId,
                        Amount = ticket.Price,
                        PaymentMethod = "Card",
                        Status = "Paid",
                        PaidAt = DateTime.Now,
                        TransactionRef = transactionRef
                    });
                }

                ticket.Status = "Booked";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Stripe payment successful! Your tickets are booked.";

            return RedirectToAction(
                "Index", "Payments", new { area = "Customer" });
        }


        [HttpGet]
        public IActionResult StripeCancel(int tripId)
        {
            TempData["Success"] =
                "Payment was cancelled. No successful payment was recorded.";

            return RedirectToAction(
                "Index", "Trips", new { area = "Admin" });
        }


    }
}
