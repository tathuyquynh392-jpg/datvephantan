
using Microsoft.AspNetCore.SignalR;

namespace TicketApi.Hubs;

public class BookingHub : Hub
{
    // Cho phép Client theo dõi một đơn đặt vé
    public async Task WatchBooking(Guid bookingId)
    {
        string groupName = $"booking:{bookingId}";

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName
        );
    }
}
