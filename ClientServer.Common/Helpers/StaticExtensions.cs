using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;

namespace ClientServer.Common.Helpers
{
    public  static class StaticExtensions
    {
        /// <summary>
        /// получает описание enum 
        /// </summary>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            var attribArray = fieldInfo.GetCustomAttributes(false);

            var descriptionAttribute =
                attribArray.FirstOrDefault(item => item is DescriptionAttribute) as DescriptionAttribute;

            return descriptionAttribute != null ? descriptionAttribute.Description : enumObj.ToString();
        }


        /// <summary>
        /// записывает объект в поток переданного клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"></param>
        public static void PutDataToStream(this TcpClient client, object obj)
        {
            var stream = client.GetStream();

            var data = Helper.ObjectToByteArray(obj);

            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// получает объект из потока
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static object GetDataFromStream(this TcpClient client)
        {
            var stream = client.GetStream();

            if (stream.CanRead)
            {
                var bytes = new byte[client.ReceiveBufferSize];

                if (stream.Read(bytes, 0, client.ReceiveBufferSize) > 0)
                    return Helper.ByteArrayToObject(bytes);

                return null;
            }

            return null;
        }

    }
}
