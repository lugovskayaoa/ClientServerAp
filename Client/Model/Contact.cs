using System.Collections.ObjectModel;
using System.Net;
using ClientServer.Common.Model;
using GalaSoft.MvvmLight;

namespace Client.Model
{
    public class Contact : NetworkClient 
    {
       /* public IPAddress IpAddress { get; set; } 
        public int Port { get; set; }*/

       /* public EndPoint Adress { get; set; }

        public string Name { get; set; }*/

        public Contact(string name, EndPoint adress)  : base(name, adress)
        {
            /*IpAddress = address;
            Port = port;*/
           /* Name = name;
            Adress = adress;*/

            Dialog = new ObservableCollection<Message>();
        }
        public ObservableCollection<Message> Dialog { get; set; }
}
}
