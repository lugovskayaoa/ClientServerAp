using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Enum
{
    public enum ServerEventEnum
    {
        [Description("Старт")]
        Start,

        [Description("Стоп")]
        Stop,

        [Description("Ошибка")]
        Error
    }
}
