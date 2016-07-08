using System;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Implementations.Base;

namespace ClientServer.Common.Model.Implementations
{
    [Serializable()]
    public class NetworkMessageState : BaseNetworkObject
    {
        public NetworkMessageState(bool isDelivered) : base(NetworkObjectTypes.MessageDelivered)
        {
            IsDelivered = isDelivered;
        }

        public bool IsDelivered { get; set; }
    }
}
