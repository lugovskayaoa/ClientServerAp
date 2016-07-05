using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Input;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Server.Model;
using Server.View;

namespace Server.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TcpListener _tcpListener;
        private IPEndPoint _ipEndPoint;
        private int _maxThreadsCount;
        private int _port;
        private IPAddress _ipAddress;
        private Thread _thread;
        private TimeSpan _timeSpan;
        private bool _isServerRun;
        private NetworkInterface _networkInterface;
        private ClientOnServer _selectedClientOnServer;
        private string _serverName;

        public MainViewModel()
        {
            _ipAddress = IPAddress.Parse("127.0.0.1"); 
            _port = 45902;

           // ShowSettings();

            _timeSpan = new TimeSpan(0, 0, 2); //время, через которое будет проходить проверка активности клиента 
            
            ServerEvents = new ObservableCollection<ServerEvent>();

            ClientsOnServer = new ObservableCollection<ClientOnServer>();

           // _tcpListener = new TcpListener(_ipEndPoint);

            _tcpListener = new TcpListener(_ipAddress, _port);
            StartServer();
        }

        #region Properties

        public ObservableCollection<ServerEvent> ServerEvents { get; set; }

        public ObservableCollection<ClientOnServer> ClientsOnServer { get; set; }

        public ClientOnServer SelectedClientOnServer
        {
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
            get { return new RelayCommand(StartServer, () => !_isServerRun); }
        }

        public RelayCommand StopServerCommand
        {
            get { return new RelayCommand(StopServer, () => _isServerRun); }
        }

        public RelayCommand SettingsCommand
        {
            get { return new RelayCommand(ShowSettings);}
        }

        public RelayCommand RemoveClientCommand
        {
            get { return new RelayCommand(RemoveClientCommandExecuted);}
        }

        public RelayCommand CloseWindowCommand
        {
            get { return new RelayCommand(CloseConnection); }
        }

        #endregion

        private void ShowSettings()
        {
            var settingsDialogVm = new SettingsViewModel(_serverName, _port, _networkInterface);

            var settingsDialog = new ServerSettingsView{DataContext = settingsDialogVm};

            if (settingsDialog.ShowDialog() == true)
            {
                ServerName = settingsDialogVm.Name; 
                _port = settingsDialogVm.Port;
                _networkInterface = settingsDialogVm.SelectedNetworkInterface;

                var ipAddresses = _networkInterface.GetIPProperties().UnicastAddresses;
                _ipEndPoint = new IPEndPoint(ipAddresses[0].Address, _port);
            }
        }

        private void StartServer()
        {
            try
            {
                _tcpListener.Start();

                _isServerRun = true;                    
                _maxThreadsCount = Environment.ProcessorCount * 4;

                ThreadPool.SetMaxThreads(_maxThreadsCount, _maxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

                _thread = new Thread(AcceptConnections);
                _thread.Start();

                ServerEvents.Add(new ServerEvent("Старт", DateTime.Now, "Выполнено"));
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent("Старт", DateTime.Now, "Не выполнено"));
                HandleError(e.ToString());
            }

        }

        private void AcceptConnections(Object stateInfo)
        {
            while (true)
            {
                ThreadPool.QueueUserWorkItem(ClientThread, _tcpListener.AcceptTcpClient());
            }
        }

        private void ClientThread(Object stateInfo)
        {
            var client = (TcpClient)stateInfo;

            //получение имени клиента
           /* var nameStream = client.GetStream();

            byte[] bytes = new byte[client.ReceiveBufferSize];

            nameStream.Read(bytes, 0, client.ReceiveBufferSize);
            var newClientName = (String)Helper.ByteArrayToObject(bytes);*/

            var newClientName = (String)GetDataFromStream(client);          

            var clientOnServer = new ClientOnServer(newClientName, client);

            //clientOnServer.ConnectionTimer.Elapsed += delegate { DisconnectInactiveClient(clientOnServer); };

            App.Current.Dispatcher.Invoke(delegate
            {               
                ClientsOnServer.Add(new ClientOnServer(newClientName, client));
            });    

            //передаем новому клиенту список клиентов сети
            var networkClients = new ObservableCollection<NetworkClient>();

            foreach (var c in ClientsOnServer)
            {
                if (c.TcpClient != client)
                    networkClients.Add(new NetworkClient(c.Name, c.TcpClient.Client.RemoteEndPoint));
            }
    
            var stream = client.GetStream();

            var data = Helper.ObjectToByteArray(networkClients);

            stream.Write(data, 0, data.Length);

            //передаем остальным клиентам сети данные нового клиента
            foreach (var c in ClientsOnServer)
            {
                if (c.TcpClient.Client.RemoteEndPoint == client.Client.RemoteEndPoint)
                    continue;

                NetworkStream tcpClientStream = c.TcpClient.GetStream();

                var tcpClientData = Helper.ObjectToByteArray(new NetworkClient(newClientName, client.Client.RemoteEndPoint));

                tcpClientStream.Write(tcpClientData, 0, tcpClientData.Length);
            }

            RouteMessage(client);

        }

        private void RouteMessage(TcpClient client)
        {
            while (client.Connected)
            {
               /* NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[client.ReceiveBufferSize];*/

                var response = GetDataFromStream(client);

               // if (stream.Read(bytes, 0, client.ReceiveBufferSize) > 0)
                if (response != null)
                {
                    //var response = Helper.ByteArrayToObject(bytes);

                    //прием тектового сообщения
                    if (response is NetworkMessage)
                    {
                        var message = (NetworkMessage) response;

                        var clientOnServer =
                            ClientsOnServer.FirstOrDefault(
                                x => Equals(x.TcpClient.Client.RemoteEndPoint, message.FromPoint));

                        if (clientOnServer != null)
                        {
                            clientOnServer.LastActivityTime = DateTime.Now;

                            SaveMessageToFile(message, clientOnServer.Name);
                        }

                        SendMessage(message);                        
                    }
                    //прием информации о изменении статуса клиента
                    if (response is NetworkClient)
                    {
                        var networkClient = (NetworkClient) response;

                        if (!networkClient.IsOnline)
                        {
                            RemoveClient(networkClient);
                        }
                    }

                }            
            }
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

        private void SendMessage(NetworkMessage message)
        {
            var netClient = ClientsOnServer.FirstOrDefault(x => Equals(x.TcpClient.Client.RemoteEndPoint, message.ToPoint));

            if (netClient != null)
            {
                NetworkStream stream = netClient.TcpClient.GetStream();

                var data = Helper.ObjectToByteArray(message);

                stream.Write(data, 0, data.Length);
            }

        }

        private void StopServer()
        {
            try
            {
                if (_tcpListener != null)
                {
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        _tcpListener.Server.Close();
                        _tcpListener.Stop();
                        _thread.Abort();
                        _isServerRun = false;
                        ServerEvents.Add(new ServerEvent("Стоп", DateTime.Now, "Выполнено"));                    
                    });
                }
            }
            catch (Exception e)
            {
                ServerEvents.Add(new ServerEvent("Стоп", DateTime.Now, "Не выполнено"));
                HandleError(e.ToString());
            }

        }

        private void HandleError(string error)
        {
            ServerEvents.Add(new ServerEvent("Ошибка", DateTime.Now, error));
        }

        private void SaveMessageToFile(NetworkMessage message, string userName)
        {
            var logMessage = DateTime.Now + " \"" + userName + "\" " + " \"" + message.Text + "\" ";

            var path = Directory.GetCurrentDirectory();

            using (var file =
            new StreamWriter(path + @"\Log.txt", true))
            {
                file.WriteLine(logMessage);
            }
        }

        private void DisconnectInactiveClient(ClientOnServer client)
        {

                if (!(client.LastActivityTime.Add(_timeSpan).CompareTo(DateTime.Now) > 0))
                {                   
                    var networkClient = new NetworkClient(client.Name, client.TcpClient.Client.RemoteEndPoint);
                    RemoveClient(networkClient);

                   
                    //client.TcpClient.Close();
                   
                    //ClientsOnServer.Remove(client);
                }   
        }

        private void RemoveClientCommandExecuted()
        {
           // RemoveClient(SelectedClientOnServer);
        }

        private void RemoveClient(NetworkClient networkClient)
        {
            var clientOnServer = ClientsOnServer.FirstOrDefault(x => x.TcpClient.Client.RemoteEndPoint.Equals(networkClient.Adress));

            App.Current.Dispatcher.Invoke(delegate
            {
                clientOnServer.TcpClient.Close();
                ClientsOnServer.Remove(clientOnServer);
            });

            foreach (var c in ClientsOnServer)
            {
                var tcpClientStream = c.TcpClient.GetStream();

                var tcpClientData = Helper.ObjectToByteArray(networkClient);

                tcpClientStream.Write(tcpClientData, 0, tcpClientData.Length);
            }            
        }

        private void CloseConnection()
        {
            StopServer();
            _tcpListener = null;
        }
    }
}
