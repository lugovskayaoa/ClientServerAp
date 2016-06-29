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
        private string _serverIP;
        private string _clientName;
        private Contact _selectedUser;
        private string _text;

        public MainViewModel()
        {
            _serverPort = 45880;
            _serverIP = "127.0.0.1";

            _client = new TcpClient(_serverIP, _serverPort);

            _clientName = _client.Client.LocalEndPoint.ToString();

            //отправка имени
            var nameStream = _client.GetStream();

            var data = Helper.ObjectToByteArray(_clientName);

            nameStream.Write(data, 0, data.Length);

            Thread ctThread = new Thread(ReceiveMessage);
            ctThread.Start();

        }

        #region Properties

        public string ServerIP
        {
            get { return _serverIP; }
            set { Set(() => ServerIP, ref _serverIP, value); }
        }

        public int ServerPort
        {
            get { return _serverPort; }
            set { Set(() => ServerPort, ref _serverPort, value); }
        }

        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
        }

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

            var stream = _client.GetStream();

            var message = new NetworkMessage((IPEndPoint)_client.Client.LocalEndPoint, (IPEndPoint)SelectedUser.Adress, Text); 

            var data = Helper.ObjectToByteArray(message);

            stream.Write(data, 0, data.Length);

            Text = "";
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                var respond = GetDataFromStream(_client);

                //получение списка клиентов
                if (respond is ObservableCollection<NetworkClient>)
                {
                    var networkClients = (ObservableCollection<NetworkClient>)respond;

                    Contacts = new ObservableCollection<Contact>();

                    foreach (var networkClient in networkClients)
                    {
                        Contacts.Add(new Contact(networkClient.Name, networkClient.Adress));
                    }                    
                }

                //получение сообщения
                if (respond is NetworkMessage)
                {
                    var message = (NetworkMessage) respond;

                    App.Current.Dispatcher.Invoke(delegate 
                    {
                        var receiver = Contacts.FirstOrDefault(x => Equals(x.Adress, message.FromPoint));

                        if (receiver != null)
                        {
                            receiver.Dialog.Add(new Message(false, message.Text));
                        }
                    });                    
                }

                //получение информации о клиенте
                if (respond is NetworkClient)
                {
                    var networkClient = (NetworkClient)respond;

                    App.Current.Dispatcher.Invoke(delegate 
                    {
                        var existingClient = Contacts.FirstOrDefault(x => Equals(x.Adress, networkClient.Adress));

                        if (existingClient != null)
                        {
                            Contacts.Remove(existingClient);
                        }
                        else
                        {
                            Contacts.Add(new Contact( networkClient.Name, networkClient.Adress));
                        }                            
                    });
                }

            }
        }

        private void SendData(TcpClient client, object obj)
        {
            var nameStream = client.GetStream();

            var data = Helper.ObjectToByteArray(obj);

            nameStream.Write(data, 0, data.Length);            
        }

        private object GetDataFromStream(TcpClient client)
        {
            var stream = client.GetStream();

            byte[] bytes = new byte[client.ReceiveBufferSize];

            stream.Read(bytes, 0, client.ReceiveBufferSize);

            return Helper.ByteArrayToObject(bytes);           
        }
    }
}
