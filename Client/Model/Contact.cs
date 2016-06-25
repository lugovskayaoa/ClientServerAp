using System.Collections.ObjectModel;
using System.Net;
using GalaSoft.MvvmLight;

namespace Client.Model
{
    public class Contact : ViewModelBase
    {
        public IPAddress IpAddress { get; set; } 
        public int Port { get; set; }

        public IPEndPoint Adress { get; set; }

        public Contact(IPEndPoint adress)
        {
            /*IpAddress = address;
            Port = port;*/

            Adress = adress;

            Dialog = new ObservableCollection<Message>();
        }
        public ObservableCollection<Message> Dialog { get; set; }
}
}
