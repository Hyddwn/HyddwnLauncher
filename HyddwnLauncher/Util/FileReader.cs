using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HyddwnLauncher.Util
{
    public class FileReader : IDisposable, IEnumerable<string>, IEnumerable
    {
        private readonly StreamReader _streamReader;

        public FileReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find '" + filePath + "'.");
            FilePath = filePath;
            RelativePath = Path.GetDirectoryName(Path.GetFullPath(filePath));
            _streamReader = new StreamReader(filePath);
        }

        public FileReader(Stream stream)
        {
            _streamReader = new StreamReader(stream);
        }

        public string FilePath { get; }

        public string RelativePath { get; }

        public int CurrentLine { get; protected set; }

        public void Dispose()
        {
            _streamReader.Close();
        }

        public IEnumerator<string> GetEnumerator()
        {
            string line;
            while ((line = _streamReader.ReadLine()) != null)
            {
                ++CurrentLine;
                line = line.Trim();
                if (!string.IsNullOrWhiteSpace(line) && line.Length >= 2 && line[0] != 33 && line[0] != 59 &&
                    line[0] != 35 && !line.StartsWith("//") && !line.StartsWith("--"))
                    yield return line;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}