using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    public interface INexonApi
    {
        Task<bool> GetAccessToken(string username, string password, string guid);
        Task<int> GetLatestVersion();
        Task<string> GetNxAuthHash();
        void HashPassword(ref string password);
        bool IsAccessTokenValid(string guid);
    }
}