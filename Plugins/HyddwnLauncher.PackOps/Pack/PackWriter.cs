using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HyddwnLauncher.PackOps.Pack.Util;

namespace HyddwnLauncher.PackOps.Pack
{
    public class PackWriter : IDisposable
    {
        private readonly FileStream _bodyStream;
        private readonly string _outFile;
        private readonly List<PackListEntry> _packList;
        private readonly TempFileScope _tempOutput;
        private PackHeader _packHeader;
        private int _version;

        public PackWriter(string outFile, int version)
        {
            _outFile = outFile;
            _packHeader = new PackHeader();
            _packList = new List<PackListEntry>();
            _tempOutput = new TempFileScope();
            _version = version;
            _bodyStream = File.Open(_tempOutput.Filename, FileMode.Create);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void WriteDirect(Stream stream, string fileName, int seed, int compressedSize, int decompressedSize,
            bool compress, DateTime creationTime, DateTime modifyDate, DateTime accessDate)
        {
            var outputStart = _bodyStream.Position;

            stream.CopyTo(_bodyStream);

            _packList.Add(new PackListEntry(fileName, new PackHeader(), null)
            {
                Seed = (uint) seed,
                IsCompressed = compress,

                CompressedSize = (uint) compressedSize,
                DecompressedSize = (uint) decompressedSize,

                DataOffset = (uint) outputStart,

                FullName = fileName,
                FileName = Path.GetFileName(fileName),

                FileTime1 = creationTime.ToFileTimeUtc(),
                FileTime2 = creationTime.ToFileTimeUtc(),
                FileTime3 = accessDate.ToFileTimeUtc(),
                FileTime4 = modifyDate.ToFileTimeUtc(),
                FileTime5 = modifyDate.ToFileTimeUtc()
            });
        }

        /// <summary>
        ///     Encodes a string for storage in a pack file. Adds null terminal.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.Byte[].</returns>
        private byte[] EncodeName(string name)
        {
            var nameRaw = Encoding.UTF8.GetBytes(name);

            var requiredLength = nameRaw.Length + 1; // +1 for null terminal

            if (requiredLength <= 0x10 * 4 - 1)
            {
                var scale = (byte) (requiredLength / 0x10);
                var buffer = new byte[(scale + 1) * 0x10];
                buffer[0] = scale;

                Buffer.BlockCopy(nameRaw, 0, buffer, 1, nameRaw.Length);

                return buffer;
            }

            if (requiredLength <= 0x60 - 1)
            {
                var buffer = new byte[0x60];
                buffer[0] = 4;

                Buffer.BlockCopy(nameRaw, 0, buffer, 1, nameRaw.Length);

                return buffer;
            }
            else
            {
                var buffer = new byte[requiredLength + 1 + sizeof(int)];
                buffer[0] = 5;
                BitConverter.GetBytes(requiredLength).CopyTo(buffer, 1);

                Buffer.BlockCopy(nameRaw, 0, buffer, 1 + sizeof(int), nameRaw.Length);

                return buffer;
            }
        }

        public void Pack()
        {
            var outputStream = File.Open(_outFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            _packHeader.Signature = new byte[] {0x50, 0x41, 0x43, 0x4B, 0x02, 0x01, 0x00, 0x00};
            _packHeader.D1 = 1;
            _packHeader.Sum = (uint) _packList.Count;
            _packHeader.DataPath = "data\\";
            _packHeader.FileTime1 = DateTime.UtcNow.ToFileTimeUtc();
            _packHeader.FileTime2 = DateTime.UtcNow.ToFileTimeUtc();

            _packHeader.FileCount = (uint) _packList.Count;
            _packHeader.HeaderLength = 0;
            _packHeader.DataLength = (uint) _bodyStream.Position;
            _packHeader.BlankLength = 0;
            _packHeader.Zero = new byte[16];

            var infos = new List<Tuple<byte[], PackageItemInfo>>();

            foreach (var entry in _packList)
            {
                var info = new PackageItemInfo
                {
                    CompressedSize = (int) entry.CompressedSize,
                    DecompressedSize = (int) entry.DecompressedSize,
                    IsCompressed = entry.IsCompressed,
                    DataOffset = entry.DataOffset,
                    Seed = (int) entry.Seed,
                    FileTime1 = entry.FileTime1,
                    FileTime3 = entry.FileTime3,
                    FileTime5 = entry.FileTime5
                };

                var bytes = EncodeName(entry.FullName);

                infos.Add(Tuple.Create(bytes, info));
                _packHeader.HeaderLength += (uint) (bytes.Length + StructHelper.SizeOf<PackageItemInfo>());
            }

            _packHeader.WriteToStream(outputStream);
            foreach (var packInfo in infos)
            {
                outputStream.Write(packInfo.Item1, 0, packInfo.Item1.Length);
                packInfo.Item2.WriteToStream(outputStream);
            }

            _bodyStream.Position = 0;
            _bodyStream.CopyTo(outputStream);

            outputStream.Flush();
            outputStream.Close();
            outputStream.Dispose();
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tempOutput.Dispose();
                _bodyStream.Dispose();
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="PackWriter" /> class.
        /// </summary>
        ~PackWriter()
        {
            Dispose(false);
        }
    }
}