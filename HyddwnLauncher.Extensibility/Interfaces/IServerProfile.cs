using System;
using System.Collections.Generic;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represents information needed to connect to a particular server
    /// </summary>
    public interface IServerProfile
    {
        /// <summary>
        ///     The arguments that are to be passed to the client
        /// </summary>
        string Arguments { get; set; }
        /// <summary>
        ///     The IP used for the chat server
        /// </summary>
        string ChatIp { get; set; }
        /// <summary>
        ///     The port used for the chat server
        /// </summary>
        int ChatPort { get; set; }
        /// <summary>
        ///     The unique GUID for the server profile
        /// </summary>
        Guid Guid { get; set; }
        /// <summary>
        ///     Determines of the profile is an official profile.
        ///     If it is, all other files are ignored and data is acquired at runtime
        /// </summary>
        bool IsOfficial { get; set; }
        /// <summary>
        ///     The IP used for the login server
        /// </summary>
        string LoginIp { get; set; }
        /// <summary>
        ///     The port used for the login server
        /// </summary>
        int LoginPort { get; set; }
        /// <summary>
        ///     A friendly name for the server profile
        /// </summary>
        string Name { get; set; }
        /// <summary>
        ///     The URL where items to be packed will be placed
        /// </summary>
        string PackDataUrl { get; set; }
        /// <summary>
        ///     The version of the pack file *must be equal or grater than client version to invoke pack engine
        /// </summary>
        int PackVersion { get; set; }
        /// <summary>
        ///     The URL used to download a json file containing the most up to date settings
        /// </summary>
        string ProfileUpdateUrl { get; set; }
        /// <summary>
        ///     The URL used to download files that will not go into a pack file
        /// </summary>
        string RootDataUrl { get; set; }
        /// <summary>
        ///     A collection of keys and values that is used to modify the urls.xml file on the fly
        /// </summary>
        List<Dictionary<string, string>> UrlsXmlOptions { get; set; }
        /// <summary>
        ///     (NOT USED) Enable or disabled the pack engine per profile.
        /// </summary>
        bool UsePackFile { get; set; }
        /// <summary>
        ///     (NOT USED) Sets the web url for URLS.XML edits
        /// </summary>
        string WebHost { get; set; }
        /// <summary>
        ///     (NOT USED) Sets the web port for URLS.XML edits
        /// </summary>
        int WebPort { get; set; }
    }
}