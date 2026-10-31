using OnTrack.Models;

namespace OnTrack.ViewModels
{
    public class EvaluationDashboardViewModel
    {
        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }

        public int FiveStarReviews { get; set; }

        public int? SelectedRating { get; set; }

        public string Search { get; set; } = string.Empty;

        public IEnumerable<Evaluation> Evaluations { get; set; }
            = new List<Evaluation>();
    }
}