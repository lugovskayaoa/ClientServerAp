using System;
using ClientServer.Common.Enums;
using ClientServer.Common.Model.Implementations.Base;

namespace ClientServer.Common.Model.Implementations
{
    [Serializable()]
    public class NetworkCommand : BaseNetworkObject
    {
        public NetworkCommand( NetworkCommandTypes commandType) : base(NetworkObjectTypes.Command)
        {
            CommandType = commandType;
        }

        public NetworkCommandTypes CommandType { get; set; }
    }
}
