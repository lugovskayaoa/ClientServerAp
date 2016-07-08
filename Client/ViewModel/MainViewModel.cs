using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Client.Model;
using Client.View;
using ClientServer.Common.Enums;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model.Implementations;
using ClientServer.Common.Model.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private fields

        private TcpClient _client;
        private Thread _msThread;
        private int _serverPort;
        private string _serverIp;
        private string _clientName;
        private Contact _selectedUser;
        private string _text;
        private string _title;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            //_serverPort = 45905;
            // _serverIP = "127.0.0.1";

            Contacts = new ObservableCollection<Contact>();
        }

        #endregion

        #region Properties

        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
        }

        public string Title
        {
            get { return _title; }
            set { Set(() => Title, ref _title, value); }
        }

        public ObservableCollection<Contact> Contacts { get; set; }

        public Contact SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                Set(() => SelectedUser, ref _selectedUser, value);

                if (SelectedUser != null)
                    SelectedUser.HaveNewMessages = false;
            }
        }

        public string Text
        {
            get { return _text; }
            set { Set(() => Text, ref _text, value); }
        }

        #endregion

        #region Commands()

        public RelayCommand SendMessageCommand
        {
            get { return new RelayCommand(SendMessage, () => SelectedUser != null); }
        }

        public RelayCommand CloseWindowCommand
        {
            get { return new RelayCommand(CloseConnection);}
        }

        public RelayCommand SettingsCommand
        {
            get { return new RelayCommand(ShowSettings); }
        }

        #endregion

        #region Methods

        private void ShowSettings()
        {
            var settingsDialogVm = new ClientSettingsViewModel();

            var settingsDialog = new ClientSettingsView {DataContext = settingsDialogVm};

            settingsDialog.Owner = Application.Current.MainWindow;

            if (settingsDialog.ShowDialog() == true)
            {
                _serverPort = settingsDialogVm.Port;
                _serverIp = settingsDialogVm.IPAddress;

                _clientName = settingsDialogVm.Name;

                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient(_serverIp, _serverPort);

                //заголовок окна
                SetTitle();

                //отправка имени
                _client.PutDataToStream(_clientName);

                _msThread = new Thread(ReceiveMessage);
                _msThread.IsBackground = true;
                _msThread.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно соединиться с сервером.");
            }
        }

        /// <summary>
        /// отправляет текстовое сообщение
        /// </summary>
        private void SendMessage()
        {
            try
            {
                var message = new NetworkMessage((IPEndPoint) _client.Client.LocalEndPoint,
                    (IPEndPoint) SelectedUser.Adress, Text);

                _client.PutDataToStream(message);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Получение данных с сервера
        /// </summary>
        private void ReceiveMessage()
        {
            while (_msThread.IsAlive && _client.Connected)
            {
                var response = _client.GetDataFromStream();

                if (response == null)
                    return;

                var networkObject = (INetworkData) response;

                ProcessMessage(networkObject);

            }
        }

        /// <summary>
        /// Обработка полученных данных
        /// </summary>
        /// <param name="networkObject"></param>
        private void ProcessMessage(INetworkData networkObject)
        {
            switch (networkObject.NetworkObjectType)
            {
                case NetworkObjectTypes.Contacts:
                {
                    var clientsCollection = (NetworkClientsCollection) networkObject;

                    AddAllContacts(clientsCollection);
                    break;
                }
                case NetworkObjectTypes.Message:
                {
                    var message = (NetworkMessage) networkObject;

                    AddMessage(message);
                    break;
                }
                case NetworkObjectTypes.ClientInfo:
                {
                    var networkClient = (NetworkClient) networkObject;

                    AddContact(networkClient);
                    break;
                }
                case NetworkObjectTypes.MessageDelivered:
                {
                    var messageState = (NetworkMessageState) networkObject;
                    var isDelivered = messageState.IsDelivered;

                    SetMessageState(isDelivered);

                    break;
                }
                case NetworkObjectTypes.Command:
                {
                    var networkCommand = (NetworkCommand) networkObject;

                    if (networkCommand.CommandType == NetworkCommandTypes.Disconnect)
                    {
                        CloseConnection();

                    }
                    if (networkCommand.CommandType == NetworkCommandTypes.ServerStop)
                    {
                        ClientStop();
                    }
                    break;
                }
            }
        }

        private void AddMessage(NetworkMessage message)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var receiver = Contacts.FirstOrDefault(x => Equals(x.Adress, message.FromPoint));

                if (receiver != null)
                {
                    receiver.Dialog.Add(new Message(false, message.Text, receiver.Name));

                    if (receiver != SelectedUser)
                    {
                        receiver.HaveNewMessages = true;
                    }
                }
            });
        }

        private void AddAllContacts(NetworkClientsCollection clientsCollection)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var networkClient in clientsCollection.NetworkClients)
                {
                    Contacts.Add(new Contact(networkClient.Name, networkClient.Adress));
                }
            });
        }

        private void AddContact(NetworkClient networkClient)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var existingClient = Contacts.FirstOrDefault(x => Equals(x.Adress, networkClient.Adress));

                if (existingClient != null)
                {
                    Contacts.Remove(existingClient);
                }
                else
                {
                    Contacts.Add(new Contact(networkClient.Name, networkClient.Adress));
                }
            });
        }

        private void SetMessageState(bool isDelivered)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                SelectedUser.Dialog.Add(new Message(true, Text, ClientName, isDelivered));
                Text = "";
            });
        }

        /// <summary>
        /// Отправляет информацию о клиенте на сервер
        /// </summary>
        private void SendStateInfo()
        {
            var networkClient = new NetworkClient(ClientName, _client.Client.LocalEndPoint, false);

            _client.PutDataToStream(networkClient);
        }

        /// <summary>
        /// Отсылает сообщение об отключении на сервер и закрывает соединение
        /// </summary>
        private void CloseConnection()
        {
            try
            {
                if (_client != null && _client.Connected)
                {
                    SendStateInfo();

                    _msThread.Abort();
                }
            }
            finally
            {
                if (_client != null)
                {
                    _client.GetStream().Close();
                    _client.Close();
                    _client = null;

                    SetTitle();
                }
            }
        }

        /// <summary>
        /// Закрывает соединение
        /// </summary>
        private void ClientStop()
        {
            try
            {
                if (_client != null && _client.Connected)
                {
                    _msThread.Abort();
                }
            }
            finally
            {
                if (_client != null)
                {
                    _client.GetStream().Close();
                    _client.Close();
                    _client = null;

                    SetTitle();
                }
            }
        }

        /// <summary>
        /// устанавливает заголовок окна
        /// </summary>
        private void SetTitle()
        {
            if (_client == null || !_client.Connected)
                Title = _clientName + " (Отключен)";
            else
            {
                Title = _clientName + " (Подключен)";
            }
        }

        #endregion
    }
}