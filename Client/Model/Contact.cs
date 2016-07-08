using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Client.Annotations;

namespace Client.Model
{
    public sealed class Contact : INotifyPropertyChanged
    {
        private bool _haveNewMessages;

        public Contact(string name, EndPoint adress) 
        {
            Name = name;
            Adress = adress;
            Dialog = new ObservableCollection<Message>();
        }

        public string Name { get; set; }

        public EndPoint Adress { get; set; }

        public ObservableCollection<Message> Dialog { get; set; }

        public bool HaveNewMessages { 
            get { return _haveNewMessages; }
            set
            {
                _haveNewMessages = value;
                OnPropertyChanged();
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
