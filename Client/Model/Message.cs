using System;

namespace Client.Model
{
    public class Message
    {
        public Message(bool isIncoming, string text)
        {
            IsIncoming = isIncoming;
            User = IsIncoming ? ">>" : "<<"; 
            Text = text;
            Time = DateTime.Now;
        }
        public bool IsIncoming;

        public string User { get; set; }

        public string Text { get; set; }

        public DateTime Time { get; set; }
    }
}
