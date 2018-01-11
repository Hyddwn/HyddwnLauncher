using System;
using System.IO;

namespace HyddwnLauncher.PackOps.Pack.Util
{
    /// <summary>
    ///     Helper class that automatically deletes a file when this class goes out of scope
    /// </summary>
    internal class TempFileScope : IDisposable
    {
        public TempFileScope()
            : this(Path.GetTempFileName())
        {
        }

        public TempFileScope(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; set; }

        public void Dispose()
        {
            try
            {
                File.Delete(Filename);
            }
            catch
            {
            }

            GC.SuppressFinalize(this);
        }

        ~TempFileScope()
        {
            Dispose();
        }
    }
}