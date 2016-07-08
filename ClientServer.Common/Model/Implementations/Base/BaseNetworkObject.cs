using System;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Interfaces;

namespace ClientServer.Common.Model.Implementations.Base
{
    [Serializable()]
    public class BaseNetworkObject : INetworkData
    {
        protected BaseNetworkObject(NetworkObjectTypes objectType)
        {
            NetworkObjectType = objectType;
        }

        public NetworkObjectTypes NetworkObjectType { get; private set; }
    }
}
