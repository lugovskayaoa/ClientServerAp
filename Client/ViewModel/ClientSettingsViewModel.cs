using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Client.ViewModel
{
    public class ClientSettingsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Private fields

        private string _ipAddress;
        private int _port;
        private string _name;

        #endregion

        #region Properties

        // ReSharper disable once InconsistentNaming
        public string IPAddress
        {
            get { return _ipAddress; }
            set { Set(() => IPAddress, ref _ipAddress, value); }
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
            return Port >= 49152 && Port <= 65535 &&
                   !string.IsNullOrEmpty(IPAddress) &&
                   !string.IsNullOrEmpty(Name);
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
                if (columnName == "IPAddress")
                {
                    if (string.IsNullOrEmpty(IPAddress))
                        Error = "IP не должен быть пустым";
                    return Error;

                }
                return null;
            }
        }

        public string Error { get; set; }

        #endregion
    }
}
