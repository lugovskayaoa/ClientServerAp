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

namespace Server.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TcpListener _server;
        private int _maxThreadsCount;
        private int _port;
        private IPAddress _ipAddress;
        private ObservableCollection<NetworkClient> _networkClients;

        public MainViewModel()
        {
            _ipAddress = IPAddress.Parse("127.0.0.1");
            _port = 45880;

            StartServer();
        }

        #region Commands

      /*  public RelayCommand StartServerCommand
        {
            get { return new RelayCommand(StartServer);}
        }

        public RelayCommand StopServerCommand
        {
            get { return new RelayCommand(StopServer); }
        }
        */
        #endregion

        private void StartServer()
        {
            _server = new TcpListener(_ipAddress, _port);
            _server.Start();

            _networkClients = new ObservableCollection<NetworkClient>();

            _maxThreadsCount = Environment.ProcessorCount * 4;

            ThreadPool.SetMaxThreads(_maxThreadsCount, _maxThreadsCount);
            ThreadPool.SetMinThreads(2, 2);

            ThreadPool.QueueUserWorkItem(AcceptConnections);

            //AcceptConnections();
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

            _networkClients.Add(new NetworkClient("имя", client));

            var clientList = new ObservableCollection<NetworkClient>(_networkClients.Where(x => x.TcpClient.Client.RemoteEndPoint != client.Client.RemoteEndPoint).ToList());

            //передаем новому клиенту список клиентов сети
            var stream = client.GetStream();

            var data = Helper.ObjectToByteArray(clientList);

            stream.Write(data, 0, data.Length);

            //передаем остальным клиентам сети данные нового клиента
            foreach (var netClient in _networkClients)
            {
                if (netClient.TcpClient.Client.RemoteEndPoint == client.Client.RemoteEndPoint)
                    continue;

                NetworkStream tcpClientStream = netClient.TcpClient.GetStream();

                var tcpClientData = Helper.ObjectToByteArray(netClient);

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
            var netClient = _networkClients.FirstOrDefault(x => Equals(x.TcpClient.Client.RemoteEndPoint, message.ToPoint));

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
            }
        }
    }
}
