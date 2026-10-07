namespace OnTrack.Models
{
    public class Station
    {
        public int StationId { get; set; }
        public string StationName { get; set; }
        public string City { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public ICollection<TripStop> TripStops { get; set; }

        public ICollection<Ticket> FromTickets { get; set; }

        public ICollection<Ticket> ToTickets { get; set; }

    }
}
