namespace HyddwnLauncher.Extensibility.Interfaces
{
    public interface IPackEngine
    {
        void Pack(string inputDir, string outputFile, uint version, int level = 9);
    }
}
