using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Server.Model;

namespace Server.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TcpListener _server;
        private int _maxThreadsCount;
        private int _port;
        private IPAddress _ipAddress;

        public MainViewModel()
        {
            _ipAddress = IPAddress.Parse("127.0.0.1");
            _port = 45880;

            ServerEvents = new ObservableCollection<ServerEvent>();
            StartServer();
        }

        #region Properties

        public ObservableCollection<ServerEvent> ServerEvents { get; set; }

        public ObservableCollection<ClientOnServer> ClientsOnServer { get; set; }
 
        #endregion

        #region Commands

        public RelayCommand StartServerCommand
        {
            get { return new RelayCommand(StartServer);}
        }

        public RelayCommand StopServerCommand
        {
            get { return new RelayCommand(StopServer); }
        }
        
        #endregion

        private void StartServer()
        {
            _server = new TcpListener(_ipAddress, _port);
            _server.Start();

            ClientsOnServer = new ObservableCollection<ClientOnServer>();

            _maxThreadsCount = Environment.ProcessorCount * 4;

            ThreadPool.SetMaxThreads(_maxThreadsCount, _maxThreadsCount);
            ThreadPool.SetMinThreads(2, 2);

            ThreadPool.QueueUserWorkItem(AcceptConnections);

            ServerEvents.Add(new ServerEvent("Старт", DateTime.Now));
        }

        private void AcceptConnections(Object stateInfo)
        {
            while (true)
            {
                /* TcpClient client = _server.AcceptTcpClient();

                 */
                ThreadPool.QueueUserWorkItem(ClientThread, _server.AcceptTcpClient());

            }
        }

        private void ClientThread(Object stateInfo)
        {
            var client = (TcpClient)stateInfo;

            //получение имени клиента
            var nameStream = client.GetStream();

            byte[] bytes = new byte[client.ReceiveBufferSize];

            nameStream.Read(bytes, 0, client.ReceiveBufferSize);

            var newClientName = (String)Helper.ByteArrayToObject(bytes);

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
            while (true)
            {
                NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[client.ReceiveBufferSize];

                stream.Read(bytes, 0, client.ReceiveBufferSize);

                var message = (NetworkMessage)Helper.ByteArrayToObject(bytes);

                SendMessage(message);
            }
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
            if (_server != null)
            {
                _server.Stop();
                ServerEvents.Add(new ServerEvent("Стоп", DateTime.Now));
            }
        }

        private void HandleError(string error)
        {
            ServerEvents.Add(new ServerEvent("Ошибка", DateTime.Now, error));
        }
    }
}
