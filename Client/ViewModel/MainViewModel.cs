using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
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
        private string _ipAddress;
        private string _clientName;
        private Contact _selectedUser;
        private string _text;

        public MainViewModel()
        {
            _serverPort = 45880;
            _ipAddress = "127.0.0.1";
            _clientName = "имя";

            _client = new TcpClient(_ipAddress, _serverPort);

            //Client = new NetworkClient(_clientName, _client);

            NetworkStream stream = _client.GetStream();

            byte[] bytes = new byte[_client.ReceiveBufferSize];

            // Read can return anything from 0 to numBytesToRead. 
            // This method blocks until at least one byte is read.
            stream.Read(bytes, 0, _client.ReceiveBufferSize);

            var networkClients = (ObservableCollection<NetworkClient>)Helper.ByteArrayToObject(bytes);

            Contacts = new ObservableCollection<Contact>();

            foreach (var networkClient in networkClients)
            {
                Contacts.Add(new Contact(networkClient.TcpClient.Client.RemoteEndPoint, networkClient.Name));
            }

            Thread ctThread = new Thread(ReceiveMessage);
            ctThread.Start();

        }

        #region Properties

        public ObservableCollection<Contact> Contacts { get; set; }

        public NetworkClient Client { get; set; }

        public Contact SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                Set(() => SelectedUser, ref _selectedUser, value);
            }
        }

        public string Text
         {
             get { return _text; } 
             set
             {
                 Set(() => Text, ref _text, value);
             }
         }

        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
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

            NetworkStream stream = _client.GetStream();

            var message = new NetworkMessage((IPEndPoint)_client.Client.LocalEndPoint, (IPEndPoint)SelectedUser.Adress, Text); 

            var data = Helper.ObjectToByteArray(message);

            stream.Write(data, 0, data.Length);

            Text = "";
        }

        private void ReceiveMessage()
        {
            while (true)
            {                           
                NetworkStream stream = _client.GetStream();

                byte[] bytes = new byte[_client.ReceiveBufferSize];

                stream.Read(bytes, 0, _client.ReceiveBufferSize);

                var respond = Helper.ByteArrayToObject(bytes);

                if (respond is NetworkMessage)
                {
                    var message = (NetworkMessage) respond;

                    App.Current.Dispatcher.Invoke((Action)delegate 
                    {
                        var receiver = Contacts.FirstOrDefault(x => Equals(x.Adress, message.FromPoint));

                        if (receiver != null)
                        {
                            receiver.Dialog.Add(new Message(false, message.Text));
                        }
                    });                    
                }
                if (respond is NetworkClient)
                {
                    var networkClient = (NetworkClient)Helper.ByteArrayToObject(bytes);

                    App.Current.Dispatcher.Invoke((Action)delegate 
                    {
                        var existingClient = Contacts.FirstOrDefault(x => Equals(x.Adress, networkClient.TcpClient.Client.RemoteEndPoint));

                        if (existingClient != null)
                        {
                            Contacts.Remove(existingClient);
                        }
                        else
                        {
                            Contacts.Add(new Contact(networkClient.TcpClient.Client.RemoteEndPoint, networkClient.Name));
                        }                            
                    });
                }

            }
        }

    }
}
