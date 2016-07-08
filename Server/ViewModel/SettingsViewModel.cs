using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Server.ViewModel
{
    public class SettingsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Private fields

        private int _port;
        private string _name;

        private NetworkInterface _selectedNetworkInterface;
        private ObservableCollection<NetworkInterface> _networkInterfaces;

        #endregion

        #region Constructors

        public SettingsViewModel(string name, int port, NetworkInterface networkInterface)
        {
            Name = name;
            Port = port;

            NetworkInterfaces = new ObservableCollection<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces());
            SelectedNetworkInterface = networkInterface;

        }

        #endregion

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

        #region Commands

        public RelayCommand SaveCommand
        {
            get { return new RelayCommand(Save, CanSave); }
        }

        private void Save()
        {

        }

        private bool CanSave()
        {
            //диапазон частных портов
            return Port > 49152 && Port < 65535 && SelectedNetworkInterface != null && !string.IsNullOrEmpty(Name);
        }

        #endregion

        #region IDataErrorInfo Members

        public string this[string columnName]
        {
            get
            {
                Error = null;

                if (columnName == "Name")
                {
                    if (string.IsNullOrEmpty(Name))
                        Error = "Название не должно быть пустым";
                    return Error;

                }
                if (columnName == "Port")
                {
                    if (!(Port >= 49152 && Port <= 65535))
                        Error = "Номер порта должен быть в диапазоне от 49152 до 65535";
                    return Error;

                }
                if (columnName == "SelectedNetworkInterface")
                {
                    if (SelectedNetworkInterface == null)
                        Error = "Необходимо выбрать сетевой интерфейс";
                    return Error;

                }
                return null;
            }
        }

        public string Error { get; private set; }

        #endregion
    }
}
