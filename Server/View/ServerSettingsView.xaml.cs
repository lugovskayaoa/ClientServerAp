using System.Windows;

namespace Server.View
{
    /// <summary>
    /// Логика взаимодействия для ServerSettingsView.xaml
    /// </summary>
    public partial class ServerSettingsView : Window
    {
        public ServerSettingsView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
