using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer.Common.Model
{
    [Serializable()]
    public class NetworkClient
    {
        public NetworkClient(string name, TcpClient client)
        {
            Name = name;
            TcpClient = client;
        }
        public string Name { get; set; }

        public TcpClient TcpClient { get; set; }

    }
}
