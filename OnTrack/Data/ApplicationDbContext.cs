using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OnTrack.Models;
using System.Reflection.Emit;

namespace OnTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
        }
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.FromStation)
                .WithMany(s => s.FromTickets)
                .HasForeignKey(t => t.FromStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.ToStation)
                .WithMany(s => s.ToTickets)
                .HasForeignKey(t => t.ToStationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<TripStop> TripStops { get; set; }
        public DbSet<TripLocation> TripLocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Payment> Payments { get; set; }


    }
}
