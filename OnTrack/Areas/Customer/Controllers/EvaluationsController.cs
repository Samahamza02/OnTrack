
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationService _evaluationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EvaluationsController(
            IEvaluationService evaluationService,
            UserManager<ApplicationUser> userManager)
        {
            _evaluationService = evaluationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string sortBy = "newest",
            int? stars = null)
        {
            var userId = _userManager.GetUserId(User);

            var allEvaluations = await _evaluationService.GetAllAsync();

            // Only evaluations belonging to the logged-in customer
            var myEvaluations = allEvaluations
                .Where(e => e.Ticket != null &&
                            e.Ticket.UserId == userId)
                .ToList();

            // Statistics: calculated before applying filters
            ViewBag.TotalReviews = myEvaluations.Count;

            ViewBag.AverageRating = myEvaluations.Any()
                ? myEvaluations.Average(e => e.rating)
                : 0;

            ViewBag.FiveStarTrips =
                myEvaluations.Count(e => e.rating == 5);

            // Filter by stars
            if (stars.HasValue && stars.Value >= 1 && stars.Value <= 5)
            {
                myEvaluations = myEvaluations
                    .Where(e => e.rating == stars.Value)
                    .ToList();
            }

            // Sort evaluations
            myEvaluations = sortBy switch
            {
                "oldest" => myEvaluations
                    .OrderBy(e => e.CreatedAt).ToList(),

                "highest" => myEvaluations
                    .OrderByDescending(e => e.rating).ToList(),

                "lowest" => myEvaluations
                    .OrderBy(e => e.rating).ToList(),

                _ => myEvaluations
                    .OrderByDescending(e => e.CreatedAt).ToList()
            };

            ViewBag.SortBy = sortBy;
            ViewBag.Stars = stars;

            return View(myEvaluations);
        }
    }
}
