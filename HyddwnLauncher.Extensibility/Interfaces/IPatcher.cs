using System.ComponentModel;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represent a Patcher. A Patcher is responsible for updating and getting launch arguments
    /// </summary>
    public interface IPatcher : INotifyPropertyChanged
    {
        /// <summary>
        ///     The Client Profile the patcher uses for it's operations
        /// </summary>
        IClientProfile ClientProfile { get; set; }

        /// <summary>
        ///     The current server profile the patcher uses for it's operations
        /// </summary>
        IServerProfile ServerProfile { get; set; }

        /// <summary>
        ///     Determines the type of patcher. Not currently in use.
        /// </summary>
        string PatcherType { get; set; }

        /// <summary>
        ///     The Patcher API Context
        /// </summary>
        PatcherContext PatcherContext { get; set; }

        /// <summary>3.
        ///     Performs an update check
        /// </summary>
        /// <returns>A boolean representing whether an update is available</returns>
        Task<bool> RepairInstallAsync();

        /// <summary>
        ///     Instructs the patcher to read the version
        /// </summary>
        /// <returns>An int representing the current version</returns>
        int ReadVersion();

        /// <summary>
        ///     Instructs the patcher to write the version
        /// </summary>
        /// <param name="version"></param>
        void WriteVersion(int version);

        /// <summary>
        ///     Instructs the patcher to acquire and return the proper launch arguments
        /// </summary>
        /// <returns>A string representing the launch arguments</returns>
        Task<string> GetLauncherArgumentsAsync();

        /// <summary>
        ///     Get the maintenance status. 
        ///     If an error occurs, it will return false.
        /// </summary>
        /// <returns>A boolean representing whether the game is in maintenance</returns>
        Task<bool> GetMaintenanceStatusAsync();
    }
}
