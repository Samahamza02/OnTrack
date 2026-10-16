using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class Station
    {

        public int StationId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Station name")]
        public string StationName { get; set; } = "";

        [Required, StringLength(100)]
        public string City { get; set; } = "";

        [Range(-90, 90)]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        public ICollection<TripStop> TripStops { get; set; } = new List<TripStop>();
        public ICollection<Ticket> FromTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> ToTickets { get; set; } = new List<Ticket>();
        //public int StationId { get; set; }
        //public string StationName { get; set; }
        //public string City { get; set; }
        //public decimal Latitude { get; set; }
        //public decimal Longitude { get; set; }

        //public ICollection<TripStop> TripStops { get; set; }

        //public ICollection<Ticket> FromTickets { get; set; }

        //public ICollection<Ticket> ToTickets { get; set; }

    }
}
