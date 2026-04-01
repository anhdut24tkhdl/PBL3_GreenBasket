using System.Collections.Generic;

namespace PPL3_Banhangonline.Models
{
    public class ChatViewModel
    {
        public string UserMessage { get; set; } = "";
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}