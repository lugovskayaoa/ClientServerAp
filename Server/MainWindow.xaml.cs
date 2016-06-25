using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using ClientServer.Common.Helpers;

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

            AcceptConnections();
        }

        private void AcceptConnections()
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

            NetworkStream stream = client.GetStream();

            var clientList = new ObservableCollection<EndPoint>(_tcpClients.Select(x => x.Client.RemoteEndPoint).ToList());

            var data = Helper.ObjectToByteArray(clientList);

            stream.Write(data, 0, data.Length);
        }

        private void ReceiveMessage(TcpClient client)
        {
            /*   // Buffer for reading data
               Byte[] bytes = new Byte[256];
               String data;

               NetworkStream stream = client.GetStream();

               int i;

               while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
               {
                   // Translate data bytes to a ASCII string.
                   data = Encoding.ASCII.GetString(bytes, 0, i);
                   Console.WriteLine("Received: {0}", data);

                   // Process the data sent by the client.
                   data = data.ToUpper();

                   byte[] msg = Encoding.ASCII.GetBytes(data);

                   // Send back a response.
                   stream.Write(msg, 0, msg.Length);
                   Console.WriteLine("Sent: {0}", data);
               }            */
            NetworkStream stream = client.GetStream();

            byte[] bytes = new byte[client.ReceiveBufferSize];

            stream.Read(bytes, 0, client.ReceiveBufferSize);

            var message = Helper.ByteArrayToObject(bytes);

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
