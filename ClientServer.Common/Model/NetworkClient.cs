﻿using System;
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
        public NetworkClient(string name, EndPoint adress)
        {
            Name = name;
            Adress = adress;
        }
        public string Name { get; set; }

        public EndPoint Adress { get; set; }

    }
}
