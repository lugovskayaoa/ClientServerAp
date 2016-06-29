using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    public class ServerEvent
    {
        public string Name { get; set; }

        public DateTime Time { get; set; }

        public string Description { get; set; }

        public ServerEvent(string name, DateTime time, string description = null)
        {
            Name = name;
            Time = time;
            Description = description;
        }
    }
}
