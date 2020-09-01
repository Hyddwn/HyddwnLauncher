using System;
using System.ComponentModel;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represents a ClientProfile
    /// </summary>
    public interface IClientProfile : INotifyPropertyChanged
    {
        /// <summary>
        ///     The file path of the mabinogi executable
        /// </summary>
        string Location { get; set; }

        /// <summary>
        ///     The friendly name of the client profile
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     The Unique GUID for the profile
        /// </summary>
        string Guid { get; }

        /// <summary>
        ///     The locale for the Mabinogi instance represented by this file.
        /// </summary>
        string Localization { get; }

        /// <summary>
        ///     Additional argument appended to the end of the arguments list.
        /// </summary>
        string Arguments { get; set; }

        /// <summary>
        ///     The username set by the user on their profile
        /// </summary>
        string ProfileUsername { get; }

        /// <summary>
        ///     The profile picture uri as report by the account system.
        /// </summary>
        string ProfileImageUri { get; }

        /// <summary>
        ///     The last known id token which is used to refresh the access token
        /// </summary>
        string LastIdToken { get; set; }

        /// <summary>
        ///     The last time the token was refreshed
        /// </summary>
        DateTime LastRefreshTime { get; set; }

        /// <summary>
        ///     The time in seconds before the token is expired
        /// </summary>
        int TokenExpirationTimeFrame { get; set; }

        /// <summary>
        ///     Determines if the token sure be kept in order to
        ///     refresh logins instead of requiring the username or password
        /// </summary>
        bool AutoLogin { get; set; }
    }
}