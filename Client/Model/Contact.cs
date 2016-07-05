using System.Collections.ObjectModel;
using System.Net;
using ClientServer.Common.Model;

namespace Client.Model
{
    public class Contact : NetworkClient 
    {
        public Contact(string name, EndPoint adress)  : base(name, adress)
        {
            Dialog = new ObservableCollection<Message>();
        }
        public ObservableCollection<Message> Dialog { get; set; }
    }
}
