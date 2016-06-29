using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    public class ClientOnServer
    {
        public ClientOnServer(string name, TcpClient client)
        {
            Name = name;
            TcpClient = client;
        }
        public string Name { get; set; }

        public TcpClient TcpClient { get; set; }
    }
}
