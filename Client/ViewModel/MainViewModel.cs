using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Client.Model;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TcpClient _client;
        private int _serverPort;
        private int _clientPort;
        private string _ipAddress;

        public MainViewModel()
        {
            _serverPort = 5888;
            _clientPort = 6888;

            _ipAddress = "127.0.0.1";

            _client = new TcpClient(_ipAddress, _serverPort);

            NetworkStream stream = _client.GetStream();

            byte[] bytes = new byte[_client.ReceiveBufferSize];

            // Read can return anything from 0 to numBytesToRead. 
            // This method blocks until at least one byte is read.
            stream.Read(bytes, 0, _client.ReceiveBufferSize);

            var clientlist = (ObservableCollection<EndPoint>)Helper.ByteArrayToObject(bytes);

            Contacts = new ObservableCollection<Contact>();

            foreach (var client in clientlist)
            {
                var iPEndPoint = (IPEndPoint)client;
                Contacts.Add(new Contact(iPEndPoint));
            }
        }

        #region Properties

        public ObservableCollection<Contact> Contacts { get; set; }

        private Contact _selectedUser;

        public Contact SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                Set(() => SelectedUser, ref _selectedUser, value);
            }
        }

        private string _text;
        public string Text
         {
             get { return _text; } 
             set
             {
                 Set(() => Text, ref _text, value);
             }
         }

        #endregion

        #region Commands()

        public RelayCommand SendMessageCommand
        {
            get { return new RelayCommand(SendMessage);}
        }
        #endregion

        private void SendMessage()
        {
            SelectedUser.Dialog.Add(new Message(true, Text));
            Text = "";

            NetworkStream stream = _client.GetStream();

            var message = new NetworkMessage(SelectedUser.Adress, new IPEndPoint(_ipAddress, _port), ); 

            var data = Helper.ObjectToByteArray(message);

            stream.Write(data, 0, data.Length);
            
        }

    }
}
