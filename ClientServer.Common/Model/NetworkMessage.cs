using System;
using System.Net;

namespace ClientServer.Common.Model
{
    [Serializable()]
    public class NetworkMessage
    {
        public IPEndPoint FromPoint;

        public IPEndPoint ToPoint;

        public string Text;

        public NetworkMessage(IPEndPoint from, IPEndPoint to, string text)
        {
            FromPoint = from;
            ToPoint = to;
            Text = text;
        }
    }
}
