using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer.Common.Model
{
    public class NetworkMessage
    {
        public IPEndPoint FromPoint;

        public IPEndPoint ToPoint;

        public string Text;

        public NetworkMessage(IPEndPoint from, IPEndPoint to, string text)
        {
            FromPoint = from;
            ToPoint = to;
            Text = text;
        }
    }
}
