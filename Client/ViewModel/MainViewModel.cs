using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Client.Model;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TcpClient _client;
        private Thread _ctThread;
        private int _serverPort;
        private string _serverIP;
        private string _clientName;
        private Contact _selectedUser;
        private string _text;

        public MainViewModel()
        {
            _serverPort = 45902;
            _serverIP = "127.0.0.1";

            try
            {
                _client = new TcpClient(_serverIP, _serverPort);

                _clientName = _client.Client.LocalEndPoint.ToString();

                //отправка имени
                SendData(_clientName);
           
                Contacts = new ObservableCollection<Contact>();            

                _ctThread = new Thread(ReceiveMessage);
                _ctThread.IsBackground = true;
                _ctThread.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно соединиться с сервером.");
            }

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
            get { return new RelayCommand(SendMessage, () => SelectedUser != null);}
        }

        public RelayCommand CloseWindowCommand
        {
            get { return new RelayCommand(CloseConnection);}
        }
        
        #endregion


        /// <summary>
        /// отправляет текстовое сообщение
        /// </summary>
        private void SendMessage()
        {
            SelectedUser.Dialog.Add(new Message(true, Text));    
        
            var message = new NetworkMessage((IPEndPoint)_client.Client.LocalEndPoint, (IPEndPoint)SelectedUser.Adress, Text); 

            SendData(message);

            Text = "";
        }

        /// <summary>
        /// Обработка полученных данных 
        /// </summary>
        private void ReceiveMessage()
        {
            while (_ctThread.IsAlive)
            {
                var respond = GetDataFromStream(_client);

                if (respond == null)
                    return;

                //получение списка клиентов
                if (respond is ObservableCollection<NetworkClient>)
                {
                    var networkClients = (ObservableCollection<NetworkClient>)respond;

                    //Contacts = new ObservableCollection<Contact>();
                    App.Current.Dispatcher.Invoke(delegate
                    {

                        foreach (var networkClient in networkClients)
                        {
                            Contacts.Add(new Contact(networkClient.Name, networkClient.Adress));
                        }
                    });
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
        /// <summary>
        /// отправка данных
        /// </summary>
        /// <param name="obj"></param>
        private void SendData(object obj)
        {
            var stream = _client.GetStream();

            var data = Helper.ObjectToByteArray(obj);

            stream.Write(data, 0, data.Length);            
        }

        /// <summary>
        /// читает данные из потока и приводит к объекту
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private object GetDataFromStream(TcpClient client)
        {
            var stream = client.GetStream();

            if (stream.CanRead)
            {
                byte[] bytes = new byte[client.ReceiveBufferSize];

                if (stream.Read(bytes, 0, client.ReceiveBufferSize) > 0)
                    return Helper.ByteArrayToObject(bytes);

                return null;
            }

            return null;
        }

        /// <summary>
        /// Отправляет информацию о клиенте на сервер
        /// </summary>
        private void SendStateInfo()
        {
            var networkClient = new NetworkClient(ClientName, _client.Client.LocalEndPoint, false);

            SendData(networkClient);        
        }

        public void CloseConnection()
        {
            SendStateInfo();

            _ctThread.Abort();
            _client.GetStream().Close();
            _client.Close();
            _client = null;

        }
    }
}
