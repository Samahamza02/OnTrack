
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;

namespace OnTrack.Hubs
{
    [Authorize]
    public class TripTrackingHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TripTrackingHub(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task JoinTrip(int tripId)
        {
            var userId = _userManager.GetUserId(Context.User!);

            if (userId == null)
                throw new HubException("You must sign in.");

            var hasTicket = await _context.Tickets.AnyAsync(t =>
                t.TripId == tripId &&
                t.UserId == userId);

            if (!hasTicket)
                throw new HubException(
                    "You are not allowed to track this trip.");

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"Trip-{tripId}");
        }

        public async Task LeaveTrip(int tripId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"Trip-{tripId}");
        }
    }
}
