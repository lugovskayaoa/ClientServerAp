using System;
using ClientServer.Common.Helpers;
using Server.Enum;

namespace Server.Model
{
    public class ServerEvent
    {
        public string Name { get; set; }

        public DateTime Time { get; set; }

        public string Description { get; set; }

        public ServerEvent(ServerEventEnum eventName, DateTime time, string description = null)
        {
            Name = eventName.GetDescription();
            Time = time;
            Description = description;
        }
    }
}
