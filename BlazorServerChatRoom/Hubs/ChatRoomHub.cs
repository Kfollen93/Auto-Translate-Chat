using Microsoft.AspNetCore.SignalR;

namespace BlazorServerChatRoom.Hubs
{
    public class ChatRoomHub : Hub
    {
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(string roomId, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
        }
    }
}
