using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Core;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for NewServerProfileUserControl.xaml
    /// </summary>
    public partial class NewServerProfileUserControl : UserControl
    {
        public static readonly DependencyProperty ServerProfileProperty = DependencyProperty.Register(
            "ServerProfile", typeof(ServerProfile), typeof(NewServerProfileUserControl),
            new PropertyMetadata(default(ServerProfile)));

        public NewServerProfileUserControl()
        {
            InitializeComponent();
        }

        public ServerProfile ServerProfile
        {
            get => (ServerProfile) GetValue(ServerProfileProperty);
            set => SetValue(ServerProfileProperty, value);
        }
    }
}