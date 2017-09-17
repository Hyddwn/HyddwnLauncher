//Copyright © 2016 exec(https://github.com/exectails)

using System;
using System.IO;
using Ionic.Zlib;

namespace HyddwnLauncher.PackOps.Pack
{
	public class PackListEntry
	{
		private readonly BinaryReader br;
		internal PackHeader header;
		private string tempPath;

		/// <summary>
		///     Creates new list entry.
		/// </summary>
		/// <param name="packFilePath"></param>
		/// <param name="packHeader"></param>
		/// <param name="binaryReader"></param>
		internal PackListEntry(string packFilePath, PackHeader packHeader, BinaryReader binaryReader)
		{
			Seed = 166;

			header = packHeader;
			br = binaryReader;
			PackFilePath = packFilePath;
		}

		internal PackListNameType NameType { get; set; }
		public string FullName { get; internal set; }
		internal uint Seed { get; set; }
		internal uint Zero { get; set; }
		internal uint DataOffset { get; set; }
		internal uint CompressedSize { get; set; }
		internal uint DecompressedSize { get; set; }
		internal bool IsCompressed { get; set; }
		internal long FileTime1 { get; set; }
		internal long FileTime2 { get; set; }
		internal long FileTime3 { get; set; }
		internal long FileTime4 { get; set; }
		internal long FileTime5 { get; set; }

		public string FileName { get; internal set; }
		public string PackFilePath { get; }

		/// <summary>
		///     Time the file was created.
		/// </summary>
		public DateTime CreationTime => DateTime.FromFileTimeUtc(FileTime1);

		/// <summary>
		///     Time the file was accessed last.
		/// </summary>
		public DateTime LastAccessTime => DateTime.FromFileTimeUtc(FileTime3);

		/// <summary>
		///     Time the file was written to last.
		/// </summary>
		public DateTime LastWriteTime => DateTime.FromFileTimeUtc(FileTime5);

		/// <summary>
		///     Extracts file to the given location.
		/// </summary>
		/// <param name="outPath">If this is null, a temp path will be generated.</param>
		/// <returns>The full path the file was extracted to.</returns>
		public string ExtractFile(string outPath = null)
		{
			if (outPath == null)
			{
				if (tempPath == null || File.Exists(tempPath))
					tempPath = Path.GetTempFileName() + Path.GetExtension(FileName);

				outPath = tempPath;
			}

			using (var fsOut = new FileStream(outPath, FileMode.Create, FileAccess.ReadWrite))
			{
				WriteData(fsOut);
			}

			return outPath;
		}

		/// <summary>
		///     Returns raw decompressed file data.
		/// </summary>
		/// <returns></returns>
		public byte[] GetData(bool decompress = true, bool decode = true)
		{
			using (var ms = new MemoryStream())
			{
				WriteData(ms, decompress, decode);
				return ms.ToArray();
			}
		}

		/// <summary>
		///     Returns raw compressed file data.
		/// </summary>
		/// <returns></returns>
		public byte[] GetCompressedData()
		{
			return GetData(false, false);
		}

		/// <summary>
		///     Returns raw decompressed data as memory stream.
		/// </summary>
		/// <returns>Stream with the data, has to be closed by the user.</returns>
		public MemoryStream GetDataAsStream()
		{
			return new MemoryStream(GetData());
		}

		/// <summary>
		///     Returns raw compressed data as memory stream.
		/// </summary>
		/// <returns>Stream with the data, has to be closed by the user.</returns>
		public MemoryStream GetCompressedDataAsStream()
		{
			return new MemoryStream(GetCompressedData());
		}

		/// <summary>
		///     Extracts the file to the temp folder and returns a file stream for it.
		/// </summary>
		/// <returns>Stream with the data, has to be closed by the user.</returns>
		public FileStream GetDataAsFileStream()
		{
			return new FileStream(ExtractFile(), FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		/// <summary>
		///     Writes decompressed data into given stream.
		/// </summary>
		/// <param name="stream"></param>
		public void WriteData(Stream stream, bool decompress = true, bool decode = true)
		{
			var start = 512 + 32 + header.HeaderLength;
			// sizeof(PackageHeader) + sizeof(PackageListHeader) + headerLength

			byte[] buffer;
			lock (br)
			{
				br.BaseStream.Seek(start + DataOffset, SeekOrigin.Begin);
				buffer = br.ReadBytes((int) CompressedSize);
			}

			if (decode)
				Decode(ref buffer);

			if (decompress)
				Decompress(buffer, stream);

			if (decompress == false && decode == false)
				stream.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		///     Decodes/Encodes buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="seed"></param>
		private void Decode(ref byte[] buffer)
		{
			var mt = new MTRandom((Seed << 7) ^ 0xA9C36DE1);

			for (var i = 0; i < buffer.Length; ++i)
				buffer[i] = (byte) (buffer[i] ^ mt.GetUInt32());
		}

		/// <summary>
		///     Decompresses buffer into stream.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="outStream"></param>
		private void Decompress(byte[] buffer, Stream outStream)
		{
			using (var zlib = new ZlibStream(outStream, CompressionMode.Decompress))
			{
				zlib.Write(buffer, 0, buffer.Length);
			}
		}
	}
}