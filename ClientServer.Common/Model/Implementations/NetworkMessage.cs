using System;
using System.Net;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Implementations.Base;

namespace ClientServer.Common.Model.Implementations
{
    [Serializable()]
    public class NetworkMessage : BaseNetworkObject
    {
        public IPEndPoint FromPoint { get; set; }

        public IPEndPoint ToPoint { get; set; }

        public string Text { get; set; }

        public NetworkMessage(IPEndPoint from, IPEndPoint to, string text) 
            : base(NetworkObjectTypes.Message)
        {
            FromPoint = from;
            ToPoint = to;
            Text = text;
        }
    }
}
