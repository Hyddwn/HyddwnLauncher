using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace HyddwnLauncher.PackOps.Pack
{
    public class PackReader : IDisposable
    {
        private readonly List<BinaryReader> binaryReaders;
        private readonly Dictionary<string, PackListEntry> entries;
        private readonly Dictionary<string, List<PackListEntry>> entriesNamed;
        private readonly List<FileStream> fileStreams;

        /// <summary>
        ///     Creates a new empty pack reader
        /// </summary>
        public PackReader()
        {
            fileStreams = new List<FileStream>();
            binaryReaders = new List<BinaryReader>();
            entries = new Dictionary<string, PackListEntry>();
            entriesNamed = new Dictionary<string, List<PackListEntry>>();
        }

        /// <summary>
        ///     Creates new pack reader for given file or folder.
        /// </summary>
        /// <param name="filePath">File or folder path. If it's a folder the reader reads all *.pack files in the top directory.</param>
        public PackReader(string filePath) : this()
        {
            FilePath = filePath;

            if (File.Exists(filePath))
                Load(filePath);
            else if (Directory.Exists(filePath))
                foreach (var path in Directory.EnumerateFiles(filePath, "*.pack", SearchOption.TopDirectoryOnly)
                    .OrderBy(a => a))
                    Load(path);
            else
                throw new ArgumentException("Path not found.");
        }

        /// <summary>
        ///     File path that was used to create this reader.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Amount of entries in all open pack files.
        /// </summary>
        public int Count => entries.Count;

        /// <summary>
        ///     Amount of open pack files.
        /// </summary>
        public int PackCount => fileStreams.Count;

        /// <summary>
        ///     Closes all file streams.
        /// </summary>
        public void Dispose()
        {
            foreach (var br in binaryReaders)
                try
                {
                    br.Close();
                }
                catch
                {
                }

            foreach (var fs in fileStreams)
                try
                {
                    fs.Close();
                }
                catch
                {
                }
        }

        /// <summary>
        ///     Closes all file streams.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        ///     Returns true if a file with the given full name exists.
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public bool Exists(string fullName)
        {
            fullName = fullName.ToLower();

            lock (entries)
            {
                return entries.ContainsKey(fullName);
            }
        }

        /// <summary>
        ///     Returns the entry with the given full name, or null if it
        ///     doesn't exist.
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public PackListEntry GetEntry(string fullName)
        {
            fullName = fullName.ToLower();

            PackListEntry result;

            lock (entriesNamed)
            {
                entries.TryGetValue(fullName, out result);
            }

            return result;
        }

        /// <summary>
        ///     Returns list of all files with the given file name.
        ///     List will be empty if none were found.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<PackListEntry> GetEntriesByFileName(string fileName)
        {
            fileName = fileName.ToLower();

            List<PackListEntry> result;

            lock (entriesNamed)
            {
                entriesNamed.TryGetValue(fileName, out result);
            }

            if (result == null)
                return new List<PackListEntry>();

            return result.ToList();
        }

        /// <summary>
        ///     Returns list of all entries.
        /// </summary>
        /// <returns></returns>
        public List<PackListEntry> GetEntries()
        {
            lock (entries)
            {
                return entries.Values.ToList();
            }
        }

        /// <summary>
        ///     Loads entries from the given pack file.
        /// </summary>
        /// <param name="filePath"></param>
        public void Load(string filePath)
        {
            int len;
            byte[] strBuffer;

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs, Encoding.ASCII);

            fileStreams.Add(fs);
            binaryReaders.Add(br);

            var header = new PackHeader();
            header.Signature = br.ReadBytes(8);
            header.D1 = br.ReadUInt32();
            header.Sum = br.ReadUInt32();
            header.FileTime1 = br.ReadInt64();
            header.FileTime2 = br.ReadInt64();
            header.Zero = new byte[16];

            strBuffer = br.ReadBytes(480);
            len = Array.IndexOf(strBuffer, (byte) 0);
            header.DataPath = Encoding.UTF8.GetString(strBuffer, 0, len);

            header.FileCount = br.ReadUInt32();
            header.HeaderLength = br.ReadUInt32();
            header.BlankLength = br.ReadUInt32();
            header.DataLength = br.ReadUInt32();
            header.Zero = br.ReadBytes(16);

            for (var i = 0; i < header.FileCount; ++i)
            {
                var entry = new PackListEntry(filePath, header, br);

                entry.NameType = (PackListNameType) br.ReadByte();

                if (entry.NameType <= PackListNameType.L64)
                {
                    var size = 0x10 * ((byte) entry.NameType + 1);
                    strBuffer = br.ReadBytes(size - 1);
                }
                else if (entry.NameType == PackListNameType.L96)
                {
                    var size = 0x60;
                    strBuffer = br.ReadBytes(size - 1);
                }
                else if (entry.NameType == PackListNameType.LDyn)
                {
                    var size = (int) br.ReadUInt32() + 5;
                    strBuffer = br.ReadBytes(size - 1 - 4);
                }
                else
                {
                    throw new Exception("Unknown entry name type '" + entry.NameType + "'.");
                }

                len = Array.IndexOf(strBuffer, (byte) 0);
                entry.FullName = Encoding.UTF8.GetString(strBuffer, 0, len);
                entry.FileName = Path.GetFileName(entry.FullName);

                entry.Seed = br.ReadUInt32();
                entry.Zero = br.ReadUInt32();
                entry.DataOffset = br.ReadUInt32();
                entry.CompressedSize = br.ReadUInt32();
                entry.DecompressedSize = br.ReadUInt32();
                entry.IsCompressed = br.ReadUInt32() == 1;
                entry.FileTime1 = br.ReadInt64();
                entry.FileTime2 = br.ReadInt64();
                entry.FileTime3 = br.ReadInt64();
                entry.FileTime4 = br.ReadInt64();
                entry.FileTime5 = br.ReadInt64();

                lock (entries)
                {
                    entries[entry.FullName.ToLower()] = entry;
                }

                lock (entriesNamed)
                {
                    var key = entry.FileName.ToLower();

                    if (!entriesNamed.ContainsKey(key))
                        entriesNamed[key] = new List<PackListEntry>();
                    entriesNamed[key].Add(entry);
                }
            }
        }

        /// <summary>
        ///     Attempts to return the path to the installed instance of Mabinogi.
        ///     Returns null if no Mabinogi folder could be found.
        /// </summary>
        /// <returns></returns>
        public static string GetMabinogiDirectory()
        {
            // TODO: More thorough search.

            var regValue = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Nexon\Mabinogi", "", "");
            if (regValue != null)
                return regValue.ToString();

            return null;
        }
    }
}