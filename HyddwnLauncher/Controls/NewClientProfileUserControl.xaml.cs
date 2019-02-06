using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
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

        public static readonly DependencyProperty CredentialUsernameProperty = DependencyProperty.Register(
            "CredentialUsername", typeof(string), typeof(NewClientProfileUserControl),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty MabiLocalizationsProperty = DependencyProperty.Register(
            "MabiLocalizations", typeof(ObservableCollection<string>), typeof(NewClientProfileUserControl),
            new PropertyMetadata(default(ObservableCollection<string>)));

        public NewClientProfileUserControl()
        {
            InitializeComponent();

            try
            {
                MabiLocalizations = new ObservableCollection<string>();

                foreach (var field in typeof(ClientLocalization).GetFields(BindingFlags.Public | BindingFlags.Static))
                    MabiLocalizations.Add((string) field.GetValue(null));
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Error occured when loading localizations.");
            }
        }

        public ClientProfile ClientProfile
        {
            get => (ClientProfile) GetValue(ClientProfileProperty);
            set => SetValue(ClientProfileProperty, value);
        }

        public string CredentialUsername
        {
            get => (string) GetValue(CredentialUsernameProperty);
            set => SetValue(CredentialUsernameProperty, value);
        }

        public ObservableCollection<string> MabiLocalizations
        {
            get => (ObservableCollection<string>) GetValue(MabiLocalizationsProperty);
            set => SetValue(MabiLocalizationsProperty, value);
        }

        private void BrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (ClientProfile == null)
            {
                ErrorWindow.IsOpen = true;
                return;
            }

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executables (*.exe)|*.exe";
            openFileDialog.InitialDirectory = "C:\\Nexon\\Library\\mabinogi\\appdata\\";
            if (openFileDialog.ShowDialog() == true)
                ClientProfile.Location = openFileDialog.FileName;
        }

        private void ClientProfileSavedCredentialsRemoveButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (ClientProfile == null) return;

            CredentialsStorage.Instance.Remove(ClientProfile.Guid);
            CredentialUsername = "";
        }
    }
}