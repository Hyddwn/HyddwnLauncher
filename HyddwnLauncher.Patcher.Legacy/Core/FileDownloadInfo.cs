using System.IO;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class FileDownloadInfo
    {
        public FileDownloadInfo(string remotePath, int size, string md5)
        {
            RemoteName = remotePath;
            Size = size;
            Md5Hash = md5;
        }

        public string RemoteName { get; protected set; }

        public string Filename => Path.GetFileName(RemoteName);

        public int Size { get; protected set; }

        public string Md5Hash { get; protected set; }
    }
}