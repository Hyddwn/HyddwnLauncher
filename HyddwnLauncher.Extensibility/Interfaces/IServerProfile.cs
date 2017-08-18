using System;
using System.Collections.Generic;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    public interface IServerProfile
    {
        string Arguments { get; set; }
        string ChatIp { get; set; }
        int ChatPort { get; set; }
        Guid Guid { get; set; }
        bool IsOfficial { get; set; }
        string LoginIp { get; set; }
        int LoginPort { get; set; }
        string Name { get; set; }
        string PackDataUrl { get; set; }
        int PackVersion { get; set; }
        string ProfileUpdateUrl { get; set; }
        string RootDataUrl { get; set; }
        List<Dictionary<string, string>> UrlsXmlOptions { get; set; }
        bool UsePackFile { get; set; }
        string WebHost { get; set; }
        int WebPort { get; set; }
    }
}