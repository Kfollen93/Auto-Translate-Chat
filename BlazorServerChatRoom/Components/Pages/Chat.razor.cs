using BlazorServerChatRoom.Models;
using BlazorServerChatRoom.Services;
using BlazorServerChatRoom.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace BlazorServerChatRoom.Components.Pages
{
    public partial class Chat
    {
        private HubConnection? hubConnection;
        private readonly List<string> messages = [];
        private string userInput = string.Empty;
        private string messageInput = string.Empty;
        private string receivedLanguage = "English";
        private bool connectionClosed = false;


        [Parameter]
        public required string ChatRoomId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(ChatRoomId) || !ChatRoomManager.ChatRoomIds.Contains(ChatRoomId))
            {
                Navigation.NavigateTo("/"); // Redirect to home page.
            }

            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri($"/chatroomhub"))
                .WithServerTimeout(TimeSpan.FromMinutes(2))
                .Build();

            hubConnection.On<string>("ReceiveMessage", async (message) =>
            {
                if (string.IsNullOrEmpty(message)) return;

                // Extract the UTC time and the message content.
                string[] messageParts = message.Split(' ');
                string utcTime = messageParts[^1];
                string messageContent = string.Join(' ', messageParts.Take(messageParts.Length - 1));

                string localTime = await JSRuntime.InvokeAsync<string>("convertUTCToLocalTime", utcTime);
                string translatedMessage = await TranslateText($"{messageContent} {localTime}", receivedLanguage);
                messages.Add(translatedMessage);
                await InvokeAsync(StateHasChanged);
            });

            await hubConnection.StartAsync();
            await hubConnection.SendAsync("JoinRoom", ChatRoomId);

            hubConnection.Closed += OnConnectionClosed;
        }

        private async Task OnConnectionClosed(Exception? _)
        {
            // Updating state is required to display the MudAlert.
            connectionClosed = true;
            await InvokeAsync(StateHasChanged);

            ChatRoomManager.RemoveChatRoomId(ChatRoomId);
            await hubConnection!.DisposeAsync();
        }

        private async Task Send()
        {
            if (hubConnection != null && !string.IsNullOrWhiteSpace(userInput) && !string.IsNullOrWhiteSpace(messageInput))
            {
                var utcTime = DateTime.UtcNow.ToString("o"); // ISO 8601.
                string formattedMessage = GetFormattedMessage(utcTime);
                await hubConnection.SendAsync("SendMessage", ChatRoomId, formattedMessage);
                messageInput = string.Empty;
            }
        }

        private string GetFormattedMessage(string utcTime)
        {
            string formattedMessage = $"{userInput}: {messageInput} {utcTime}";
            return formattedMessage;
        }

        public bool IsConnected =>
            hubConnection?.State == HubConnectionState.Connected;

        private async Task<string> TranslateText(string text, string targetLanguage)
        {
            var request = new
            {
                source = "auto",
                target = LanguageMap.Languages[targetLanguage],
                text,
                proxies = Array.Empty<string>()
            };

            var response = await httpClient.PostAsJsonAsync("https://deep-translator-api.azurewebsites.net/google/", request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonSerializer.Deserialize<TranslationResponse>(jsonString);

                return jsonObject!.translation;
            }
            else
            {
                return $"An error occured during translation. The original message was: {text}.";
            }
        }

        private async Task CopyToClipboard()
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", $"{Navigation.BaseUri}chat/{ChatRoomId}");
            Snackbar.Add("Link copied to clipboard!", Severity.Success);
        }
    }
}