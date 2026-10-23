
using System.ComponentModel.DataAnnotations;

namespace OnTrack.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int TripId { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public List<int> TicketIds { get; set; } = new();

        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Please choose a payment method.")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
