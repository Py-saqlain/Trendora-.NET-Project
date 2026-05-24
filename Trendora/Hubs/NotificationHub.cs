using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Trendora.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var role = Context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == "Admin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                    Console.WriteLine($"Admin connected: {Context.ConnectionId}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
