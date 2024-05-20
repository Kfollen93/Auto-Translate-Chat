namespace BlazorServerChatRoom.Models
{
    /// <summary>
    /// Keep the lowercase of properties to match the third-party API expected response.
    /// </summary>
    public class TranslationResponse
    {
        public required string translation { get; set; }
        public string? error { get; set; }
    }

}
