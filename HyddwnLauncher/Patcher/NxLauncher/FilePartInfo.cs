namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class FilePartInfo
    {
        public int Index { get; }
        public string PartName { get; }
        public string FileName { get; }
        public long FileSize { get; }

        public FilePartInfo(string partName, string fileName, long fileSize = 0, int index = 0)
        {
            Index = index;
            PartName = partName;
            FileName = fileName;
            FileSize = fileSize;
        }
    }
}