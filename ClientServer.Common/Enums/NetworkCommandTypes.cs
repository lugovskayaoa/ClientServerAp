using System.ComponentModel;

namespace ClientServer.Common.Enums
{
    public enum NetworkCommandTypes
    {
        [Description("Отключение клиента")]
        Disconnect,

        [Description("Остановка сервера")]
        ServerStop,
    }
}
