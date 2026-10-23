
using System.ComponentModel.DataAnnotations;
using OnTrack.Models;

namespace OnTrack.Models.ViewModels
{
    public class CreateTripViewModel
    {
        [Required]
        public int? TemplateTripId { get; set; }

        [Required]
        public DateTime? TripDate { get; set; }

        [Required]
        public int? TrainId { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        public List<CreateTripStopInput> Stops { get; set; } =
            new List<CreateTripStopInput>();
    }

    public class CreateTripStopInput
    {
        public int? StationId { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public string? Platform { get; set; }
    }
}
