using System.Threading.Tasks;
using HyddwnLauncher.Extensibility.Model;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represents the NexonAPI for plugin access
    /// </summary>
    public interface INexonApi
    {
        /// <summary>
        ///     Attempts to retrieve an access token
        /// </summary>
        /// <param name="username">The username to log in with</param>
        /// <param name="password">The SHA512 hashed password to log in with</param>
        /// <param name="clientProfile">The related client profile</param>
        /// <param name="rememberMe">Whether to remember and as a result enable auto-login</param>
        /// <returns></returns>
        Task<GetAccessTokenResponse> GetAccessToken(string username, string password, IClientProfile clientProfile, bool rememberMe);

        /// <summary>
        ///     Attempts to identify the newest version of the Mabinogi client
        /// </summary>
        /// <returns></returns>
        Task<int> GetLatestVersion();

        /// <summary>
        ///     Attempts to retrieve the NX Passport Hash
        /// </summary>
        /// <returns></returns>
        Task<string> GetNxAuthHash();

        /// <summary>
        ///     Hashs the password with SHA512 hash algorithm
        /// </summary>
        /// <param name="password">The text password to use</param>
        void HashPassword(ref string password);

        /// <summary>
        ///     Attempts to detect when the access token has expired.
        /// </summary>
        /// <param name="guid">The currently active profile GUID</param>
        /// <returns></returns>
        bool IsAccessTokenValid(string guid);

        /// <summary>
        /// Retrieves official product information about Mabinogi
        /// </summary>
        /// <returns></returns>
        Task<dynamic> GetMabinogiMetadata();
    }
}