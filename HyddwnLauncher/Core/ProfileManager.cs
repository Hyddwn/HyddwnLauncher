using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
    public class ProfileManager : INotifyPropertyChanged
    {
        private readonly string _clientProfileJson =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\clientprofiles.json";

        private readonly string _serverProfileJson =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\serverprofiles.json";

        private ObservableCollection<ClientProfile> _clientProfiles;
        private ObservableCollection<ServerProfile> _serverProfiles;

        public ProfileManager()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_clientProfileJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_clientProfileJson));

            if (!Directory.Exists(Path.GetDirectoryName(_serverProfileJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_serverProfileJson));

            Load();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ClientProfiles):
                    SaveClientProfiles();
                    break;
                case nameof(ServerProfiles):
                    SaveServerProfiles();
                    break;
            }
        }

        public ObservableCollection<ClientProfile> ClientProfiles
        {
            get => _clientProfiles;
            private set
            {
                if (Equals(value, _clientProfiles)) return;
                _clientProfiles = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ServerProfile> ServerProfiles
        {
            get => _serverProfiles;
            private set
            {
                if (Equals(value, _serverProfiles)) return;
                _serverProfiles = value;
                OnPropertyChanged();
            }
        }

        public async Task UpdateProfiles()
        {
            foreach (var serverProfile in ServerProfiles)
                await serverProfile.GetUpdates();
            SaveServerProfiles();
        }

        public void Load()
        {
            ClientProfiles = LoadClientProfiles();
            ServerProfiles = LoadServerProfiles();

            ClientProfiles.CollectionChanged += ClientProfilesOnCollectionChanged;
            ServerProfiles.CollectionChanged += ServerProfilesOnCollectionChanged;

            // Attempt to correct existing profiles with no localization set
            foreach (var clientProfile in ClientProfiles.Where(x => string.IsNullOrWhiteSpace(x.Localization)))
                clientProfile.Localization = "North America";
        }

        public ObservableCollection<ClientProfile> LoadClientProfiles()
        {
            try
            {
                var fs = new FileStream(_clientProfileJson, FileMode.Open);
                var sr = new StreamReader(fs);

                var result = JsonConvert.DeserializeObject<ObservableCollection<ClientProfile>>(sr.ReadToEnd());

                sr.Dispose();
                fs.Dispose();

                return result ?? new ObservableCollection<ClientProfile>();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.UnableToLoadClientProfileData);
                MessageBox.Show(
                    string.Format(Properties.Resources.UnableToLoadClientProfileDataMessage,
                        ex.Message), Properties.Resources.Error);
                return new ObservableCollection<ClientProfile>();
            }
        }

        public ObservableCollection<ServerProfile> LoadServerProfiles()
        {
            try
            {
                var fs = new FileStream(_serverProfileJson, FileMode.Open);
                var sr = new StreamReader(fs);

                var result = JsonConvert.DeserializeObject<ObservableCollection<ServerProfile>>(sr.ReadToEnd());

                sr.Dispose();
                fs.Dispose();

                return result ?? new ObservableCollection<ServerProfile>();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.UnableToLoadServerProfileData);
                MessageBox.Show(
                    string.Format(Properties.Resources.UnableToLoadServerProfileDataMessage,
                        ex.Message), Properties.Resources.Error);
                return new ObservableCollection<ServerProfile>();
            }
        }

        public void SaveClientProfiles()
        {
            try
            {
                var jsonFile = JsonConvert.SerializeObject(ClientProfiles, Formatting.Indented);
                jsonFile.WriteAllTextWithBackup(_clientProfileJson);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.UnableToSaveClientProfileData);
                MessageBox.Show(
                    string.Format(
                        Properties.Resources.UnableToSaveClientProfileDataMessage,
                        ex.Message), Properties.Resources.Error);
            }
        }

        public void SaveServerProfiles()
        {
            try
            {
                var jsonFile = JsonConvert.SerializeObject(ServerProfiles, Formatting.Indented);
                jsonFile.WriteAllTextWithBackup(_serverProfileJson);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.UnableToSaveServerProfileData);
                MessageBox.Show(
                    string.Format(
                        Properties.Resources.UnableToSaveServerProfileDataMessage,
                        ex.Message), Properties.Resources.Error);
            }
        }

        public void ResetClientProfiles()
        {
            ClientProfiles.Clear();
            SaveClientProfiles();
        }

        public void ResetServerProfiles()
        {
            ServerProfiles.Clear();
            ServerProfiles.Add(ServerProfile.OfficialProfile);
            SaveServerProfiles();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClientProfilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveClientProfiles();
        }

        private void ServerProfilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveClientProfiles();
        }
    }
}