namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represents an item that supports packing operations
    /// </summary>
    public interface IPackEngine
    {
        /// <summary>
        ///     Creates a pack file using the data provided
        /// </summary>
        /// <param name="inputDir"></param>
        /// <param name="outputFile"></param>
        /// <param name="version"></param>
        /// <param name="level"></param>
        void Pack(string inputDir, string outputFile, uint version, int level = 9);
    }
}