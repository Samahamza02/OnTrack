using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string SeatNumber { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime BookingDate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;

        [ForeignKey(nameof(FromStation))]
        public int FromStationId { get; set; }
        public Station FromStation { get; set; } = null!;

        [ForeignKey(nameof(ToStation))]
        public int ToStationId { get; set; }
        public Station ToStation { get; set; } = null!;

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        public Evaluation? Evaluation { get; set; }
    }
}