namespace BlazorServerChatRoom.Services
{
    public class ChatRoomManager
    {
        public readonly HashSet<string> ChatRoomIds = [];

        public bool IsValidChatRoomId(string chatRoomId)
        {
            return ChatRoomIds.Contains(chatRoomId);
        }

        public void AddChatRoomId(string chatRoomId)
        {
            ChatRoomIds.Add(chatRoomId);
        }

        public void RemoveChatRoomId(string chatRoomId)
        {
            ChatRoomIds.Remove(chatRoomId);
        }

        public int GetNumberOfCurrentActiveRooms() => ChatRoomIds.Count;
    }
}
