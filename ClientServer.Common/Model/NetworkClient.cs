using System;
using System.Net;

namespace ClientServer.Common.Model
{
    [Serializable()]
    public class NetworkClient
    {
        public NetworkClient(string name, EndPoint adress, bool isOnline = true)
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
