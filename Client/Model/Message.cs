using System;

namespace Client.Model
{
    public class Message
    {
        public Message(bool isIncoming, string text, string user, bool? isDelivered = null)
        {
            IsIncoming = isIncoming;
            User = user;
            Text = text;
            Time = DateTime.Now;
            IsDelivered = isDelivered;
        }
        public bool IsIncoming { get; set; }

        public bool? IsDelivered { get; set; }

        public string User { get; set; }

        public string Text { get; set; }

        public DateTime Time { get; set; }
    }
}
