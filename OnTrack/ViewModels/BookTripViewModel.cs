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

        [Required(ErrorMessage = "Enter the seat number")]
        public string SeatNumber { get; set; } = string.Empty;
        public List<string> AvailableSeats { get; set; } = new();
    }
}