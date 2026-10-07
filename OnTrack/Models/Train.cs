namespace OnTrack.Models
{
    public class Train
    {
        public int TrainId { get; set; }
        public string TrainNumber { get; set; }
        public string TrainName { get; set; }
        public string TrainType { get; set; }
        public ICollection<Trip> Trips { get; set; }

    }
}
