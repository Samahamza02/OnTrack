using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string TransactionRef { get; set; }

        [ForeignKey(nameof(Ticket))]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

    }
}
