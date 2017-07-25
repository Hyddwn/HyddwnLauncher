using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Util;
using Microsoft.Win32;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for NewClientProfileUserControl.xaml
    /// </summary>
    public partial class NewClientProfileUserControl : UserControl
    {
        public static readonly DependencyProperty ClientProfileProperty = DependencyProperty.Register(
            "ClientProfile", typeof(ClientProfile), typeof(NewClientProfileUserControl),
            new PropertyMetadata(default(ClientProfile)));

        public NewClientProfileUserControl()
        {
            InitializeComponent();
        }

        public ClientProfile ClientProfile
        {
            get => (ClientProfile) GetValue(ClientProfileProperty);
            set => SetValue(ClientProfileProperty, value);
        }

        private void BrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executables (*.exe)|*.exe";
            openFileDialog.InitialDirectory = "C:\\Nexon\\Library\\mabinogi\\appdata\\";
            if (openFileDialog.ShowDialog() == true)
                ClientProfile.Location = openFileDialog.FileName;
        }
    }
}