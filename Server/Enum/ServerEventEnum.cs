using System.ComponentModel;

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
