using System;
using System.Collections.Generic;
using System.Linq;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class FileDownloadInfo
    {
        public FileInfoType FileInfoType { get; }
        public List<FilePartInfo> FileParts { get; }
        public DateTime LastModifiedDateTime { get; private set; }
        public string FileName { get; }
        public long FileSize { get; }

        public long PartsFileSize
        {
            get { return FileParts.Sum(filePartInfo => filePartInfo.FileSize); }
        }

        public void SetModifiedTimeDateTime(long timeInSeconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            LastModifiedDateTime = epoch.AddSeconds(timeInSeconds).ToLocalTime();
        }

        public FileDownloadInfo(string fileName, long fileSize = 0, FileInfoType fileInfoType = FileInfoType.File)
        {
            FileName = fileName;
            FileSize = fileSize;
            FileParts = new List<FilePartInfo>();
            FileInfoType = fileInfoType;
        }
    }
}
