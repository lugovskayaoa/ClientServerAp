using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using ClientServer.Common.Helpers;
using ClientServer.Common.Model;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _server;
        private int _port;
        private string _name;
        private IPAddress _ipAddress;
        private int _maxThreadsCount;


        private ObservableCollection<TcpClient> _tcpClients;

        public MainWindow()
        {
            InitializeComponent();

            _ipAddress = IPAddress.Parse("127.0.0.1");
            _port = 5888;

            StartServer();
        }

        private void StartServer()
        {
            _server = new TcpListener(_ipAddress, _port);
            _server.Start();

            _tcpClients = new ObservableCollection<TcpClient>();

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

            _tcpClients.Add(client);
           
            var clientList = new ObservableCollection<EndPoint>(_tcpClients.Select(x => x.Client.RemoteEndPoint).ToList());

            //передаем новому клиенту список слиентов сети
            var stream = client.GetStream();

            var data = Helper.ObjectToByteArray(clientList);

            stream.Write(data, 0, data.Length);

            //передаем остальным клиентам сети данные нового клиента
            foreach (var tcpClient in _tcpClients)
            {
                if (tcpClient == client)
                    break; 
                    
                NetworkStream tcpClientStream = tcpClient.GetStream();

                var tcpClientData = Helper.ObjectToByteArray(client.Client.RemoteEndPoint);

                tcpClientStream.Write(tcpClientData, 0, tcpClientData.Length);
            }

            RouteMessage(client);

        }

        private void RouteMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            byte[] bytes = new byte[client.ReceiveBufferSize];

            stream.Read(bytes, 0, client.ReceiveBufferSize);

            var message = (NetworkMessage)Helper.ByteArrayToObject(bytes);

            SendMessage(message);

        }

        private void SendMessage(NetworkMessage message)
        {
            var client = _tcpClients.FirstOrDefault(x => Equals(x.Client.RemoteEndPoint, message.ToPoint));

            if (client != null)
            {
                NetworkStream stream = client.GetStream();

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
