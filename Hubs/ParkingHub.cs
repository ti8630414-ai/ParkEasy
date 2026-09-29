using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ParkEasy.Web.Hubs
{
    public class ParkingHub : Hub
    {
        public async Task JoinSpaceGroup(string spaceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Space_{spaceId}");
        }

        public async Task LeaveSpaceGroup(string spaceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Space_{spaceId}");
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }
}
