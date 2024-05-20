using BlazorServerChatRoom.Services;

namespace BlazorServerChatRoom.Components.Pages
{
    public partial class Home
    {
        private readonly string[] Texts =
        [
            "Auto-Translate Chat",
            "Chat automatisch übersetzen",
            "автоматический перевод чата",
            "Chat de traducción automática"
        ];
        private int currentIndex = 0;
        private string createdChatroomId = string.Empty;
        private System.Timers.Timer timer = new();
        private const int MAX_NUM_OF_ROOMS = 1;

        protected override void OnInitialized()
        {
            timer = new System.Timers.Timer
            {
                Interval = 1500,
                AutoReset = true
            };
            timer.Elapsed += async (sender, e) => await Timer_ElapsedAsync(sender!, e);
            timer.Start();
        }

        private async Task Timer_ElapsedAsync(object sender, System.Timers.ElapsedEventArgs e)
        {
            currentIndex = (currentIndex + 1) % Texts.Length;
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
        }

        private void CreateChatroom()
        {
            if (ChatRoomManager.GetNumberOfCurrentActiveRooms() < 3)
            {
                createdChatroomId = Guid.NewGuid().ToString();
                ChatRoomManager.AddChatRoomId(createdChatroomId);
                Navigation.NavigateTo($"chat/{createdChatroomId}");
            }
        }

        private bool IsMaxChatRoomsReached()
        {
            return ChatRoomManager.GetNumberOfCurrentActiveRooms() >= MAX_NUM_OF_ROOMS;
        }
    }
}