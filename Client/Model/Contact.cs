using System.Collections.ObjectModel;
using System.Net;
using GalaSoft.MvvmLight;

namespace Client.Model
{
    public class Contact 
    {
       /* public IPAddress IpAddress { get; set; } 
        public int Port { get; set; }*/

        public EndPoint Adress { get; set; }

        public string Name { get; set; }

        public Contact(EndPoint adress, string name)
        {
            /*IpAddress = address;
            Port = port;*/
            Name = name;
            Adress = adress;

            Dialog = new ObservableCollection<Message>();
        }
        public ObservableCollection<Message> Dialog { get; set; }
}
}
