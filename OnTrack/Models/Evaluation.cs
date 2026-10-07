using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class Evaluation
    {
        public int EvaluationId { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey(nameof(Ticket))]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}
