using System.Windows;

namespace Client.View
{
    /// <summary>
    /// Логика взаимодействия для ClientProperties.xaml
    /// </summary>
    public partial class ClientSettingsView : Window
    {
        public ClientSettingsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
