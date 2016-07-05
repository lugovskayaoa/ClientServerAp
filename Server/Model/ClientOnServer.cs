using System;
using System.Net.Sockets;
using Timer = System.Timers.Timer;

namespace Server.Model
{
    public class ClientOnServer
    {
        public ClientOnServer(string name, TcpClient client)
        {
            Name = name;
            TcpClient = client;
            LastActivityTime = DateTime.Now;

            ConnectionTimer = new Timer();
            ConnectionTimer.Interval = 6000;
            ConnectionTimer.Enabled = true;

        }
        public string Name { get; set; }

        public TcpClient TcpClient { get; set; }

        public DateTime LastActivityTime { get; set; }

        public Timer ConnectionTimer { get; set; }
    }
}
