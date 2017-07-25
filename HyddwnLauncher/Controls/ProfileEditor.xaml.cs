using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Util;
using MahApps.Metro.Controls.Dialogs;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor
    {
        private readonly bool _isNoProfiles;

        public ProfileEditor(ProfileManager profileManager, bool isNoProfiles = false)
        {
            _isNoProfiles = isNoProfiles;
            ProfileManager = profileManager;
            InitializeComponent();
        }

        public ProfileManager ProfileManager { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void AddItem()
        {
            var newItem = new ClientProfile {Name = "New Profile"};
            ProfileManager.ClientProfiles.Add(newItem);
            ClientProfileListBox.SelectedItem = newItem;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ClientProfileListBox.SelectedIndex == -1) return;
            ProfileManager.ClientProfiles.RemoveAt(ClientProfileListBox.SelectedIndex);
        }

        private void ClientProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProfileManager.SaveClientProfiles();
        }

        private void This_Closing(object sender, CancelEventArgs e)
        {
            ProfileManager.SaveClientProfiles();
        }

        private async void This_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isNoProfiles)
            {
                await
                    this.ShowMessageAsync("No Client Profile",
                        "You have been taken to this window because you do not have a profile configured for Mabinogi. A Client Profile represents an instace of Mabinogi that you installed. At lease one profile must be available in order to use this launcher. Once you are finished, select the profile you would like to use then close the window.");

                AddItem();
            }
        }
    }
}