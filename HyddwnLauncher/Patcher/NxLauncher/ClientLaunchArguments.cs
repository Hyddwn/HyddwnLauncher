using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class ClientLaunchArguments
    {
        public ClientLaunchArguments(string[] cmdArgs)
        {
            Code = cmdArgs.FirstOrDefault(a => a.StartsWith("code:"))?.Substring(5);
            Version = cmdArgs.FirstOrDefault(a => a.StartsWith("ver:"))?.Substring(4);
            VersionString = cmdArgs.FirstOrDefault(a => a.StartsWith("verstr:"))?.Substring(7);
            Locale = cmdArgs.FirstOrDefault(a => a.StartsWith("locale:"))?.Substring(7);
            Environment = cmdArgs.FirstOrDefault(a => a.StartsWith("env:"))?.Substring(4);
            Setting = cmdArgs.FirstOrDefault(a => a.StartsWith("setting:"))?.Substring(8);
            LogIp = cmdArgs.FirstOrDefault(a => a.StartsWith("logip:"))?.Substring(6);
            LogPort = cmdArgs.FirstOrDefault(a => a.StartsWith("logport:"))?.Substring(8);
            ChatIp = cmdArgs.FirstOrDefault(a => a.StartsWith("chatip:"))?.Substring(7);
            ChatPort = cmdArgs.FirstOrDefault(a => a.StartsWith("chatport:"))?.Substring(9);
            Passport = cmdArgs.FirstOrDefault(a => a.StartsWith("/P:"))?.Substring(3);
            BgLoader = cmdArgs.FirstOrDefault(a => a.StartsWith("-bgloader")) != null;
            Client = cmdArgs.FirstOrDefault(a => a.StartsWith("c:"))?.Substring(2);
            Username = cmdArgs.FirstOrDefault(a => a.StartsWith("u:"))?.Substring(2);
            Password = cmdArgs.FirstOrDefault(a => a.StartsWith("p:"))?.Substring(2);
        }

        public string Code { get; set; }
        public string Version { get; set; }
        public string VersionString { get; set; }
        public string Locale { get; set; }
        public string Environment { get; set; }
        public string Setting { get; set; }
        public string LogIp { get; set; }
        public string LogPort { get; set; }
        public string ChatIp { get; set; }
        public string ChatPort { get; set; }
        public string Passport { get; set; }
        public bool BgLoader { get; set; }
        public string Client { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public int IntLogPort
        {
            get => int.Parse(LogPort);
            set => LogPort = value.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Code)) sb.Append($"code:{Code} ");

            if (!string.IsNullOrWhiteSpace(Version)) sb.Append($"ver:{Version} ");

            if (!string.IsNullOrWhiteSpace(VersionString)) sb.Append($"verstr:{VersionString} ");

            if (!string.IsNullOrWhiteSpace(Locale)) sb.Append($"locale:{Locale} ");

            if (!string.IsNullOrWhiteSpace(Environment)) sb.Append($"env:{Environment} ");

            if (!string.IsNullOrWhiteSpace(Setting)) sb.Append($"setting:{Setting} ");

            if (!string.IsNullOrWhiteSpace(Passport)) sb.Append($"/P:{Passport} ");

            if (!string.IsNullOrWhiteSpace(LogIp)) sb.Append($"logip:{LogIp} ");

            if (!string.IsNullOrWhiteSpace(LogPort)) sb.Append($"logport:{LogPort} ");

            if (!string.IsNullOrWhiteSpace(ChatIp)) sb.Append($"chatip:{ChatIp} ");

            if (!string.IsNullOrWhiteSpace(ChatPort)) sb.Append($"chatport:{ChatPort} ");

            if (BgLoader) sb.Append("-bgloader ");

            return sb.ToString();
        }
    }
}
