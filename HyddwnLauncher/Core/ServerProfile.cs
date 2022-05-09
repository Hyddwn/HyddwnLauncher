using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
    public class ServerProfile : INotifyPropertyChanged, IServerProfile
    {
        private string _arguments;
        private string _chatIp;
        private int _chatPort;
        private Guid _guid;
        private bool _isOfficial;
        private string _loginIp;
        private int _loginPort;
        private string _name;
        private string _packDataUrl;
        private int _packVersion;
        private string _profileUpdateUrl;
        private string _rootDataUrl;
        private List<Dictionary<string, string>> _urlsXmlOptions;
        private bool _usePackFile;
        private string _webHost;
        private int _webPort;

        public ServerProfile()
        {
            IsOfficial = false;
            UrlsXmlOptions = new List<Dictionary<string, string>>();
            Guid = Guid.NewGuid();
        }

        public static ServerProfile OfficialProfile => new ServerProfile
        {
            Name = "Nexon Official",
            IsOfficial = true
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsOfficial
        {
            get => _isOfficial;
            set
            {
                if (value == _isOfficial) return;
                _isOfficial = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string LoginIp
        {
            get => _loginIp;
            set
            {
                if (value == _loginIp) return;
                _loginIp = value;
                OnPropertyChanged();
            }
        }

        public string Arguments
        {
            get => _arguments;
            set
            {
                if (value == _arguments) return;
                _arguments = value;
                OnPropertyChanged();
            }
        }

        public int LoginPort
        {
            get => _loginPort;
            set
            {
                if (value == _loginPort) return;
                _loginPort = value;
                OnPropertyChanged();
            }
        }

        public string ChatIp
        {
            get => _chatIp;
            set
            {
                if (value == _chatIp) return;
                _chatIp = value;
                OnPropertyChanged();
            }
        }

        // Intended for files that would belong in a pack file
        public int ChatPort
        {
            get => _chatPort;
            set
            {
                if (value == _chatPort) return;
                _chatPort = value;
                OnPropertyChanged();
            }
        }

        public string WebHost
        {
            get => _webHost;
            set
            {
                if (value == _webHost) return;
                _webHost = value;
                OnPropertyChanged();
            }
        }

        public int WebPort
        {
            get => _webPort;
            set
            {
                if (value == _webPort) return;
                _webPort = value;
                OnPropertyChanged();
            }
        }

        public int PackVersion
        {
            get => _packVersion;
            set
            {
                if (value == _packVersion) return;
                _packVersion = value;
                OnPropertyChanged();
            }
        }

        public string PackDataUrl
        {
            get => _packDataUrl;
            set
            {
                if (value == _packDataUrl) return;
                _packDataUrl = value;
                OnPropertyChanged();
            }
        }

        // Intended for files the DO NOT belong in a pack file
        public string RootDataUrl
        {
            get => _rootDataUrl;
            set
            {
                if (value == _rootDataUrl) return;
                _rootDataUrl = value;
                OnPropertyChanged();
            }
        }

        public string ProfileUpdateUrl
        {
            get => _profileUpdateUrl;
            set
            {
                if (value == _profileUpdateUrl) return;
                _profileUpdateUrl = value;
                OnPropertyChanged();
            }
        }

        public List<Dictionary<string, string>> UrlsXmlOptions
        {
            get => _urlsXmlOptions;
            set
            {
                if (Equals(value, _urlsXmlOptions)) return;
                _urlsXmlOptions = value;
                OnPropertyChanged();
            }
        }

        public Guid Guid
        {
            get => _guid;
            set
            {
                if (value.Equals(_guid)) return;
                _guid = value;
                OnPropertyChanged();
            }
        }

        public bool UsePackFile
        {
            get => _usePackFile;
            set
            {
                if (value == _usePackFile) return;
                _usePackFile = value;
                OnPropertyChanged();
            }
        }

        public async Task GetUpdatesAsync()
        {
            if (string.IsNullOrWhiteSpace(ProfileUpdateUrl)) return;

            var client = new WebClient();

            try
            {
                var profileJson = await client.DownloadStringTaskAsync(ProfileUpdateUrl);

                var serverProfile = JsonConvert.DeserializeObject<ServerProfile>(profileJson);

                Arguments = serverProfile.Arguments;

                PackVersion = serverProfile.PackVersion;
                PackDataUrl = serverProfile.PackDataUrl;
                RootDataUrl = serverProfile.RootDataUrl;
                ProfileUpdateUrl = serverProfile.ProfileUpdateUrl;

                ChatIp = serverProfile.ChatIp;
                ChatPort = serverProfile.ChatPort;
                LoginIp = serverProfile.LoginIp;
                LoginPort = serverProfile.LoginPort;
                WebHost = serverProfile.WebHost;
                WebPort = serverProfile.WebPort;

                Name = serverProfile.Name;
                IsOfficial = serverProfile.IsOfficial;

                UrlsXmlOptions = serverProfile.UrlsXmlOptions;

                Guid = serverProfile.Guid;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to update a profile!");
            }
        }

        public static ServerProfile Create(string name = "Local", string loginIp = "127.0.0.1",
            string chatIp = "127.0.0.1", string webHost = "127.0.0.1", string profileUpdateUrl = "",
            string packDataUrl = "", string rootDataUrl = "", string arguments = "", int webPort = 80,
            int loginPort = 11000, int chatPort = 8002, int packFileVersion = 0)
        {
            var serverProfile = new ServerProfile();

            serverProfile.UrlsXmlOptions.Add(new Dictionary<string, string>());
            serverProfile.UrlsXmlOptions[0].Add("UploadUIPage", $"http://{webHost}:{webPort}/ui/");
            serverProfile.UrlsXmlOptions[0].Add("UploadVisualChatPage", $"http://{webHost}:{webPort}/visual-chat/");
            serverProfile.UrlsXmlOptions[0].Add("DownloadUIAddress", $"http://{webHost}:{webPort}/user/save/ui/");
            serverProfile.UrlsXmlOptions[0].Add("UploadAvatarPage", $"http://{webHost}:{webPort}/avatar-upload/");
            serverProfile.UrlsXmlOptions[0].Add("CreateAccountPage", $"http://{webHost}:{webPort}/register/");

            serverProfile.Arguments = arguments;

            serverProfile.PackVersion = packFileVersion;

            serverProfile.PackDataUrl = packDataUrl;
            serverProfile.RootDataUrl = rootDataUrl;
            serverProfile.ProfileUpdateUrl = profileUpdateUrl;

            serverProfile.ChatIp = chatIp;
            serverProfile.ChatPort = chatPort;
            serverProfile.LoginIp = loginIp;
            serverProfile.LoginPort = loginPort;
            serverProfile.WebHost = webHost;
            serverProfile.WebPort = webPort;
            serverProfile.Name = name;

            return serverProfile;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}