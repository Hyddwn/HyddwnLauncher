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

        public static readonly DependencyProperty UserAvatarSourceProperty = DependencyProperty.Register(
            "UserAvatarSource", typeof(string), typeof(NewClientProfileUserControl), 
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ProfileUsernameProperty = DependencyProperty.Register(
            "ProfileUsername", typeof(string), typeof(NewClientProfileUserControl), 
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty MabiLocalizationsProperty = DependencyProperty.Register(
            "MabiLocalizations", typeof(ObservableCollection<MabiLocalizationData>), typeof(NewClientProfileUserControl),
            new PropertyMetadata(default(ObservableCollection<MabiLocalizationData>)));

        public NewClientProfileUserControl()
        {
            InitializeComponent();

            try
            {
                MabiLocalizations = new ObservableCollection<MabiLocalizationData>();

                foreach (var field in typeof(ClientLocalization).GetFields(BindingFlags.Public | BindingFlags.Static))
                    MabiLocalizations.Add(new MabiLocalizationData { Name = (string)field.GetValue(null) });
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.ErrorOccurredWhenLoadingLocalizations);
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

        public string ProfileUsername
        {
            get => (string)GetValue(ProfileUsernameProperty);
            set => SetValue(ProfileUsernameProperty, value);
        }

        public string UserAvatarSource
        {
            get => (string)GetValue(UserAvatarSourceProperty);
            set => SetValue(UserAvatarSourceProperty, value);
        }

        public ObservableCollection<MabiLocalizationData> MabiLocalizations
        {
            get => (ObservableCollection<MabiLocalizationData>) GetValue(MabiLocalizationsProperty);
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
            openFileDialog.Filter = Properties.Resources.ProfileOpenFileDialogFilter;
            openFileDialog.InitialDirectory = "C:\\Nexon\\Library\\mabinogi\\appdata\\";
            if (openFileDialog.ShowDialog() == true)
            {
                ClientProfile.Location = openFileDialog.FileName;
                // I shouldn't have to do this...
                LocationTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }

        }

        private void ClientProfileSavedCredentialsRemoveButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (ClientProfile == null)
            {
                ErrorWindow.IsOpen = true;
                return;
            }

            CredentialsStorage.Instance.Remove(ClientProfile.Guid);
            CredentialUsername = "";
            ProfileUsername = "";
            UserAvatarSource = "";
        }
    }
}