using Microsoft.AspNetCore.SignalR;

namespace FoodX.EmailService.Hubs;

public class EmailHub : Hub
{
    public async Task JoinEmailGroup(string userEmail)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userEmail}");
    }

    public async Task LeaveEmailGroup(string userEmail)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userEmail}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any groups this connection was part of
        await base.OnDisconnectedAsync(exception);
    }
}