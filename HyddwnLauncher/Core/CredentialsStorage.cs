using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
    internal class CredentialsStorage
    {
        public static readonly CredentialsStorage Instance = new CredentialsStorage();

        private readonly string _credentialsRoot =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher";

        private readonly string _credentialsJson;
        private readonly string _credentialsDat;

        public ObservableDictionary<string, CredentialsObject> Credentials { get; private set; }

        public CredentialsStorage()
        {
            _credentialsJson = $"{_credentialsRoot}\\credentials.json";
            _credentialsDat = $"{_credentialsRoot}\\credentials.dat";

            Directory.CreateDirectory(_credentialsRoot);

            if (File.Exists(_credentialsJson))
            {
                UpgradeStorage();
                return;
            }
            
            Credentials = Load();
        }

        private void UpgradeStorage()
        {
            try
            {
                var result =
                    JsonConvert.DeserializeObject<ObservableDictionary<string, CredentialsObject>>(File.ReadAllText(_credentialsJson));

                Credentials = result ?? new ObservableDictionary<string, CredentialsObject>();

                Save();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to load credentials storage for upgrade.");
                Credentials = new ObservableDictionary<string, CredentialsObject>();
            }
            finally
            {
                if (File.Exists(_credentialsJson))
                    File.Delete(_credentialsJson);

                if (File.Exists($"{_credentialsJson}.backup"))
                    File.Delete($"{_credentialsJson}.backup");

                Log.Info("Credentials upgraded");
            }
        }

        private ObservableDictionary<string, CredentialsObject> Load()
        {
            try
            {
                if (!File.Exists(_credentialsDat))
                    return new ObservableDictionary<string, CredentialsObject>();

                var encryptedBytes = File.ReadAllBytes(_credentialsDat);

                var entropy = this.CreateEntropyFromDeviceId();

                var plainBytes = ProtectedData.Unprotect(encryptedBytes, entropy, DataProtectionScope.LocalMachine);

                var jsonData = Encoding.UTF8.GetString(plainBytes);

                var result = JsonConvert.DeserializeObject<ObservableDictionary<string, CredentialsObject>>(jsonData);

                return result ?? new ObservableDictionary<string, CredentialsObject>();
            }
            catch (Exception)
            {
                return new ObservableDictionary<string, CredentialsObject>();
            }
        }

        public void Save()
        {
            try
            {
                var jsonFile = JsonConvert.SerializeObject(Credentials, Formatting.Indented);
                
                var initialBytes = Encoding.UTF8.GetBytes(jsonFile);
                this.PadToMultipleOf(ref initialBytes, 16);

                var entropy = this.CreateEntropyFromDeviceId();
                
                var encryptedBytes = ProtectedData.Protect(initialBytes, entropy, DataProtectionScope.LocalMachine);

                encryptedBytes.WriteAllBytesWithBackup(_credentialsDat);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.UnableToSaveCredentialData);
                MessageBox.Show(string.Format(Properties.Resources.UnableToSaveCredentialDataMessage, ex.Message), "Error");
            }
        }

        public void Reset()
        {
            Credentials.Clear();
            Save();
        }

        public void Add(string guid, string username, string password)
        {
            var credentials = GetCredentialsForProfile(guid);

            if (credentials != null)
            {
                credentials = new CredentialsObject(username, password);
                Credentials[guid] = credentials;
            }
            else
            {
                Credentials.Add(guid, new CredentialsObject(username, password));
            }

            Save();
        }

        public void Update(string guid, string username, string password)
        {
            var credentials = GetCredentialsForProfile(guid);
            if (credentials == null) return;

            credentials = new CredentialsObject(username, password);

            Credentials[guid] = credentials;
            Save();
        }

        public void Remove(string guid)
        {
            try
            {
                Credentials.Remove(guid);
            }
            catch
            {
            }

            Save();
        }

        public CredentialsObject GetCredentialsForProfile(string guid)
        {
            Credentials.TryGetValue(guid, out var credentials);

            return credentials;
        }

        private byte[] CreateEntropyFromDeviceId()
        {
            var entropy = Encoding.UTF8.GetBytes(NexonApi.GetDeviceUuid());

            return entropy;
        }

        private void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }
    }

    internal class CredentialsObject
    {
        private string _password;
        private string _username;

        public CredentialsObject(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username
        {
            get => Encoding.UTF8.GetString(Convert.FromBase64String(_username));
            private set => _username = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public string Password
        {
            get => Encoding.UTF8.GetString(Convert.FromBase64String(_password));
            private set => _password = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }
}