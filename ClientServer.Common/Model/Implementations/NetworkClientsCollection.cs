using System;
using System.Collections.ObjectModel;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Implementations.Base;

namespace ClientServer.Common.Model.Implementations
{
    [Serializable()]
    public class NetworkClientsCollection : BaseNetworkObject
    {
        public NetworkClientsCollection(ObservableCollection<NetworkClient> networkClients)
            : base(NetworkObjectTypes.Contacts)
        {
            NetworkClients = networkClients;
        }

        public ObservableCollection<NetworkClient> NetworkClients;
    }
}
