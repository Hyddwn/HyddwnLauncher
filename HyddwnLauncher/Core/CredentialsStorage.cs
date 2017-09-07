using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
    internal class CredentialsStorage
    {
        public ObservableDictionary<string, CredentialsObject> CredntialsObjects { get; }

        public readonly string _credentialsJson = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\credentials.json";

        public static readonly CredentialsStorage Instance = new CredentialsStorage();

        public CredentialsStorage()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_credentialsJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_credentialsJson));

            CredntialsObjects = Load();
        }

        private ObservableDictionary<string, CredentialsObject> Load()
        {
            try
            {
                var fs = new FileStream(_credentialsJson, FileMode.Open);
                var sr = new StreamReader(fs);

                var result = JsonConvert.DeserializeObject<ObservableDictionary<string, CredentialsObject>>(sr.ReadToEnd());

                sr.Dispose();
                fs.Dispose();

                return result;
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
                var json = JsonConvert.SerializeObject(CredntialsObjects, Formatting.Indented);
                if (File.Exists(_credentialsJson))
                    File.Delete(_credentialsJson);

                File.WriteAllText(_credentialsJson, json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to save server credential data");
                MessageBox.Show("Unable to save credential data.\r\n\r\n" + ex.Message, "Error");
            }
        }

        public void Reset()
        {
            CredntialsObjects.Clear();
            Save();
        }

        public void Add(string guid, string username, string password)
        {
            var credentials = GetCredentialsForProfile(guid);

            if (credentials != null)
            {
                credentials = new CredentialsObject(username, password);
                CredntialsObjects[guid] = credentials;
            }
            else
            {
                CredntialsObjects.Add(guid, new CredentialsObject(username, password));
            }

            Save();
        }

        public void Update(string guid, string username, string password)
        {
            var credentials = GetCredentialsForProfile(guid);
            if (credentials == null) return;

            credentials = new CredentialsObject(username, password);

            CredntialsObjects[guid] = credentials;
            Save();
        }

        public void Remove(string guid)
        {
            try
            {
                CredntialsObjects.Remove(guid);
            }
            catch { }
            Save();
        }

        public CredentialsObject GetCredentialsForProfile(string guid)
        {
            CredntialsObjects.TryGetValue(guid, out var credentials);

            return credentials;
        }
    }

    internal class CredentialsObject
    {
        private string _username;
        private string _password;

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
