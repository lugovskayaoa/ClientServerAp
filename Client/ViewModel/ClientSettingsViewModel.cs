using GalaSoft.MvvmLight;

namespace Client.ViewModel
{
    public class ClientSettingsViewModel : ViewModelBase
    {
        private string _ipAdress;
        private int _port;
        private string _name;

        public ClientSettingsViewModel(string ip, int port)
        {
            IPAddress = ip;
            Port = port;
        }
        #region Properties

        public string IPAddress
        {
            get { return _ipAdress; }
            set { Set(() => IPAddress, ref _ipAdress, value); }
        }

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
        #endregion
    }
}
