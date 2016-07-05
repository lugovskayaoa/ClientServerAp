using System;
using Server.Enum;

namespace Server.Model
{
    public class ServerEvent
    {
        public string Name { get; set; }

        public DateTime Time { get; set; }

        public string Description { get; set; }

        public ServerEvent(string name, DateTime time, string description)
        {
          /*  switch (eventName)
            {
                case ServerEventEnum.Start:
                {
                    Name = "";
                    break;
                }
            }*/
            
            Name = name;
            Time = time;
            Description = description;
        }
    }
}
