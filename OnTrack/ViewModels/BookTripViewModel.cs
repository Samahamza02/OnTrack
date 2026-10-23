
using System.ComponentModel.DataAnnotations;

namespace OnTrack.Models.ViewModels
{
    public class BookTripViewModel
    {
        public int TripId { get; set; }

        public string? RouteName { get; set; }

        public DateTime TripDate { get; set; }

        public string? TrainNumber { get; set; }

        public decimal Price { get; set; }

        public List<TripStop> Stops { get; set; } = new();

        [Required(ErrorMessage = "Choose the boarding station")]
        public int FromStationId { get; set; }

        [Required(ErrorMessage = "Choose the destination station")]
        public int ToStationId { get; set; }

        // Multiple selected seats
        [MinLength(1, ErrorMessage = "Please choose at least one seat.")]
        public List<string> SeatNumbers { get; set; } = new();

        // Seats that are still available
        public List<string> AvailableSeats { get; set; } = new();

        [Required]
        public string TravelClass { get; set; } = "Economy";
    }
}
