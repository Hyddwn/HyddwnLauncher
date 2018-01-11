using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
    public class ProfileManager
    {
        private readonly string _clientProfileJson =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\clientprofiles.json";

        private readonly string _serverProfileJson =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\serverprofiles.json";

        public ProfileManager()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_clientProfileJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_clientProfileJson));

            if (!Directory.Exists(Path.GetDirectoryName(_serverProfileJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_serverProfileJson));

            Load();
        }

        public ObservableCollection<ClientProfile> ClientProfiles { get; private set; }
        public ObservableCollection<ServerProfile> ServerProfiles { get; private set; }

        public void UpdateProfiles()
        {
            foreach (var serverProfile in ServerProfiles)
                serverProfile.GetUpdates();
            SaveServerProfiles();
        }

        public void Load()
        {
            ClientProfiles = LoadClientProfiles();
            ServerProfiles = LoadServerProfiles();
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

                return result;
            }
            catch (Exception)
            {
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

                return result;
            }
            catch (Exception)
            {
                return new ObservableCollection<ServerProfile>();
            }
        }

        public void SaveClientProfiles()
        {
            try
            {
                var json = JsonConvert.SerializeObject(ClientProfiles, Formatting.Indented);
                if (File.Exists(_clientProfileJson))
                    File.Delete(_clientProfileJson);

                File.WriteAllText(_clientProfileJson, json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to save client profile data");
                MessageBox.Show("Unable to save ClientProfile data.\r\n\r\n" + ex.Message, "Error");
            }
        }

        public void SaveServerProfiles()
        {
            try
            {
                var json = JsonConvert.SerializeObject(ServerProfiles, Formatting.Indented);
                if (File.Exists(_serverProfileJson))
                    File.Delete(_serverProfileJson);

                File.WriteAllText(_serverProfileJson, json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to save server profile data");
                MessageBox.Show("Unable to save ServerProfile data.\r\n\r\n" + ex.Message, "Error");
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
    }
}