using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OnTrack.Models
{
    public enum TripStatus
    {
        Scheduled,
        OnTime,
        Delayed,
        Completed,
        Cancelled
    }
    public class Trip
    {
        public int TripId { get; set; }
        public string? RouteName { get; set; }
        public TripStatus Status { get; set; }
        public DateTime TripDate { get; set; }
        public decimal Price { get; set; }
        [ForeignKey(nameof(Train))]
        public int TrainId { get; set; }
        [ValidateNever]
        public Train Train { get; set; } = null!;

        public ICollection<TripStop> TripStops { get; set; } = new List<TripStop>();

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    }
}
