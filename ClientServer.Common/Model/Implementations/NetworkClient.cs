using System;
using System.Net;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Implementations.Base;

namespace ClientServer.Common.Model.Implementations
{
    [Serializable()]
    public class NetworkClient : BaseNetworkObject
    {
        public NetworkClient(string name, EndPoint adress, bool isOnline = true) 
            :base(NetworkObjectTypes.ClientInfo)
        {
            Name = name;
            Adress = adress;

            IsOnline = isOnline;
        }
        public string Name { get; set; }

        public EndPoint Adress { get; set; }

        public bool IsOnline { get; set; }

    }
}
