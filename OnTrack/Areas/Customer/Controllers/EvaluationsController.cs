using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationService _evaluationService;

        public EvaluationsController(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        // GET: /Evaluations
        public async Task<IActionResult> Index()
        {
            var evaluations = await _evaluationService.GetAllAsync();

            return View(evaluations);
        }

        // GET: /Evaluations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var evaluation = await _evaluationService.GetByIdAsync(id);

            if (evaluation == null)
            {
                return NotFound();
            }

            return View(evaluation);
        }
    }
}