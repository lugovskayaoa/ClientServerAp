using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using ClientServer.Common.Enums;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model.Implementations;
using ClientServer.Common.Model.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Server.Enum;
using Server.Model;
using Server.View;

namespace Server.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private fields

        private TcpListener _tcpListener;
        private IPEndPoint _ipEndPoint;
        private int _maxThreadsCount;
        private int _port;
        private Thread _thread;
        private TimeSpan _timeSpan;
        private bool _isServerRun;
        private NetworkInterface _networkInterface;
        private ClientOnServer _selectedClientOnServer;
        private string _serverName;
        private readonly string _logFileName;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            _logFileName = Directory.GetCurrentDirectory() + @"\Log.txt";

            _timeSpan = new TimeSpan(0, 2, 0); //время, через которое будет проходить проверка активности клиента 

            ServerEvents = new ObservableCollection<ServerEvent>();

            ClientsOnServer = new ObservableCollection<ClientOnServer>();
        }

        #endregion

        #region Properties

        public ObservableCollection<ServerEvent> ServerEvents { get; set; }

        public ObservableCollection<ClientOnServer> ClientsOnServer { get; set; }

        public ClientOnServer SelectedClientOnServer {
            get { return _selectedClientOnServer; }
            set { Set(() => SelectedClientOnServer, ref _selectedClientOnServer, value); }
        }

        public string ServerName
        {
            get { return _serverName; }
            set { Set(() => ServerName, ref _serverName, value); }
        }

        #endregion

        #region Commands

        public RelayCommand StartServerCommand
        {
            get { return new RelayCommand(StartServer, () => !_isServerRun && _tcpListener != null); }
        }

        public RelayCommand StopServerCommand
        {
            get { return new RelayCommand(StopServer, () => _isServerRun); }
        }

        public RelayCommand SettingsCommand
        {
            get { return new RelayCommand(ShowSettings); }
        }

        public RelayCommand CloseWindowCommand
        {
            get { return new RelayCommand(CloseConnection); }
        }

        public RelayCommand DisconnectClientCommand
        {
            get { return new RelayCommand(DisconnectClient, () => SelectedClientOnServer != null); }
        }

        public RelayCommand OpenHistoryCommand
        {
            get { return new RelayCommand(OpenHistory);}
        }

        #endregion

        #region Methods

        private void OpenHistory()
        {
            try
            {
                Process.Start(_logFileName);
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }
        }

        private void ShowSettings()
        {
            try
            {
                var settingsDialogVm = new SettingsViewModel(_serverName, _port, _networkInterface);

                var settingsDialog = new ServerSettingsView {DataContext = settingsDialogVm};

                settingsDialog.Owner = Application.Current.MainWindow;

                if (settingsDialog.ShowDialog() == true)
                {
                    ServerName = settingsDialogVm.Name;
                    _port = settingsDialogVm.Port;
                    _networkInterface = settingsDialogVm.SelectedNetworkInterface;

                    var ipAddress = _networkInterface.GetIPProperties().UnicastAddresses.First(x => x.Address.AddressFamily == AddressFamily.InterNetwork); //получает IPv4
                    _ipEndPoint = new IPEndPoint(ipAddress.Address, _port); 

                    StopServer();
                    _tcpListener = new TcpListener(_ipEndPoint);
                    // _tcpListener = new TcpListener(_ipAddress, _port);
                    StartServer();
                }
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }

        }

        private void StartServer()
        {
            try
            {
                _tcpListener.Start();

                _isServerRun = true;

                _maxThreadsCount = Environment.ProcessorCount*4;

                ThreadPool.SetMaxThreads(_maxThreadsCount, _maxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

                _thread = new Thread(AcceptConnections);
                _thread.Start();

                ServerEvents.Add(new ServerEvent(ServerEventEnum.Start, DateTime.Now));
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }

        }

        private void AcceptConnections()
        {
            while (_isServerRun)
            {
                if (!_tcpListener.Pending())
                {
                    Thread.Sleep(500);
                    continue;
                }

                ThreadPool.QueueUserWorkItem(ClientThread, _tcpListener.AcceptTcpClient());
            }
        }

        private void ClientThread(Object stateInfo)
        {
            try
            {
                var client = (TcpClient) stateInfo;

                //получение имени клиента
                var newClientName = (string) client.GetDataFromStream();

                var clientOnServer = new ClientOnServer(newClientName, client)
                {
                    ConnectionTimer = {Interval = _timeSpan.TotalMilliseconds}  //интервал через который будет производиться проверка активности
                };

                clientOnServer.ConnectionTimer.Elapsed += delegate { DisconnectInactiveClient(clientOnServer); };
                
                Application.Current.Dispatcher.Invoke(delegate
                {
                    ClientsOnServer.Add(new ClientOnServer(newClientName, client));
                });

                //передаем новому клиенту список клиентов сети
                var networkClients = new ObservableCollection<NetworkClient>();

                foreach (var c in ClientsOnServer.Where(c => c.TcpClient != client))
                {
                    networkClients.Add(new NetworkClient(c.Name, c.TcpClient.Client.RemoteEndPoint));
                }

                client.PutDataToStream(new NetworkClientsCollection(networkClients));

                //передаем остальным клиентам сети данные нового клиента
                foreach (var c in ClientsOnServer)
                {
                    if (c.TcpClient.Client.RemoteEndPoint == client.Client.RemoteEndPoint)
                        continue;

                    var newClient = new NetworkClient(newClientName, client.Client.RemoteEndPoint);

                    c.TcpClient.PutDataToStream(newClient);
                }

                ReceiveMessage(client);
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }

        }

        /// <summary>
        /// получение данных от клиента
        /// </summary>
        /// <param name="client"></param>
        private void ReceiveMessage(TcpClient client)
        {
            while (_isServerRun && client.Connected)
            {
                var response = client.GetDataFromStream();

                if (response == null)
                {
                    return;
                }

                var networkObject = (INetworkData) response;

                ProcessMessage(networkObject, client);

            }
        }

        /// <summary>
        /// Обработка полученных данных
        /// </summary>
        /// <param name="networkObject"></param>
        /// <param name="client"></param>
        private void ProcessMessage(INetworkData networkObject, TcpClient client)
        {
            switch (networkObject.NetworkObjectType)
            {
                case NetworkObjectTypes.Message:
                {
                    var message = (NetworkMessage) networkObject;

                    ProcessTextMessage(message, client);
                    break;
                }
                case NetworkObjectTypes.ClientInfo:
                {
                    var networkClient = (NetworkClient) networkObject;

                    ProcessClient(networkClient);

                    break;
                }
            }
        }

        private void ProcessTextMessage(NetworkMessage message, TcpClient client)
        {
            var clientOnServer =
                ClientsOnServer.FirstOrDefault(
                    x => Equals(x.TcpClient.Client.RemoteEndPoint, message.FromPoint));

            if (clientOnServer != null)
            {
                clientOnServer.LastActivityTime = DateTime.Now;

                SaveMessageToLogFile(message, clientOnServer.Name);
            }

            var isDelivered = SendMessage(message);

            //отправка информации о состоянии сообщения
            client.PutDataToStream(new NetworkMessageState(isDelivered));
        }

        private void ProcessClient(NetworkClient networkClient)
        {
            if (networkClient.IsOnline) 
                return;

            var clientOnServer =
                ClientsOnServer.FirstOrDefault(
                    x => x.TcpClient.Client.RemoteEndPoint.Equals(networkClient.Adress));
            RemoveClient(clientOnServer);
        }

        private bool SendMessage(NetworkMessage message)
        {
            try
            {
                var netClient =
                    ClientsOnServer.FirstOrDefault(x => Equals(x.TcpClient.Client.RemoteEndPoint, message.ToPoint));

                if (netClient != null)
                {
                    netClient.TcpClient.PutDataToStream(message);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void StopServer()
        {
            try
            {
                if (_tcpListener != null)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        foreach (var client in ClientsOnServer)
                        {
                            client.TcpClient.PutDataToStream(new NetworkCommand(NetworkCommandTypes.ServerStop));
                        }
                        _tcpListener.Server.Close();
                        _tcpListener.Stop();
                        _isServerRun = false;

                        _thread.Abort();

                        ServerEvents.Add(new ServerEvent(ServerEventEnum.Stop, DateTime.Now));
                    });
                }
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }

        }

        private void SaveMessageToLogFile(NetworkMessage message, string userName)
        {
            var logMessage = DateTime.Now + " \"" + userName + "\" " + " \"" + message.Text + "\" ";

            using (var file =
                new StreamWriter(_logFileName, true))
            {
                file.WriteLine(logMessage);
            }
        }

        /// <summary>
        /// отключает неактивного клиента 
        /// </summary>
        /// <param name="client"></param>
        private void DisconnectInactiveClient(ClientOnServer client)
        {
            if (!(client.LastActivityTime.Add(_timeSpan).CompareTo(DateTime.Now) > 0))
            {
                client.ConnectionTimer.Enabled = false;
                client.TcpClient.PutDataToStream(new NetworkCommand(NetworkCommandTypes.Disconnect));
            }
        }

        private void DisconnectClient()
        {
            SelectedClientOnServer.TcpClient.PutDataToStream(new NetworkCommand(NetworkCommandTypes.Disconnect));
        }

        private void RemoveClient(ClientOnServer clientOnServer)
        {
            try
            {
                var networkClient = new NetworkClient(clientOnServer.Name,
                    clientOnServer.TcpClient.Client.RemoteEndPoint, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ClientsOnServer.Remove(clientOnServer);
                });

                foreach (var c in ClientsOnServer)
                {
                    c.TcpClient.PutDataToStream(networkClient);
                }
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent(ServerEventEnum.Error, DateTime.Now, e.ToString()));
            }

        }

        private void CloseConnection()
        {
            StopServer();
            _tcpListener = null;
        }

        #endregion
    }
}
