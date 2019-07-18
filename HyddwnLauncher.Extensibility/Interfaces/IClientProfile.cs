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

        string LastIdToken { get; set; }

        DateTime LastRefreshTime { get; set; }

        int TokenExpirationTimeFrame { get; set; }
    }
}