using System.ComponentModel;

namespace ClientServer.Common.Enums
{
    public enum NetworkObjectTypes
    {
        [Description("Контакты")]
        Contacts,

        [Description("Сообщение")]
        Message,

        [Description("Информация о клиенте")]
        ClientInfo,

        [Description("Информация о доставке сообщения")]
        MessageDelivered,

        [Description("Команда клиенту")]
        Command,
    }
}
