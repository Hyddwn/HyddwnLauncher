using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility
{
    public interface INexonApi
    {
        Task<bool> GetAccessToken(string username, string password);
        Task<int> GetLatestVersion();
        Task<string> GetNxAuthHash();
        void HashPassword(ref string password);
        bool IsAccessTokenValid();
    }
}