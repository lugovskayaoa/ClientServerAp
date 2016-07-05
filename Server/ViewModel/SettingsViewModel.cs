using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using GalaSoft.MvvmLight;

namespace Server.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private int _port;
        private string _name;

        private NetworkInterface _selectedNetworkInterface;
        private ObservableCollection<NetworkInterface> _networkInterfaces;

        public SettingsViewModel()
        {
            
        }
        public SettingsViewModel(string name, int port, NetworkInterface networkInterface)
        {
            Name = name;
            Port = port;

            NetworkInterfaces = new ObservableCollection<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces());
            SelectedNetworkInterface = networkInterface;

        }

        #region Properties

        public int Port
        {
            get { return _port; }
            set { Set(() => Port, ref _port, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        public NetworkInterface SelectedNetworkInterface
        {
            get { return _selectedNetworkInterface; }
            set { Set(() => SelectedNetworkInterface, ref _selectedNetworkInterface, value); }
        }

        public ObservableCollection<NetworkInterface> NetworkInterfaces 
        {
            get { return _networkInterfaces; }
            set { Set(() => NetworkInterfaces, ref _networkInterfaces, value); }
        }

        #endregion    
    }
}
