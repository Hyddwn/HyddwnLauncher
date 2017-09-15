using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HyddwnLauncher.PackOps.Pack.Util
{
    public static class StructHelper
    {
        /// <summary>
        /// Gets the bytes that represent a given struct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] GetBytes<T>(this T obj) where T : struct
        {
            var size = Marshal.SizeOf(obj);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

		/// <summary>
		/// Gets the structure from a byte array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr">The arr.</param>
		/// <param name="offset">The offset.</param>
		/// <returns>T.</returns>
		public static T GetStruct<T>(this byte[] arr, int offset) where T : struct
		{
			var handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
			var stuff = (T)Marshal.PtrToStructure(IntPtr.Add(handle.AddrOfPinnedObject(), offset), typeof(T));
			handle.Free();
			return stuff;
		}

		/// <summary>
		/// Reads a struct from a stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="file">The file.</param>
		/// <returns>T.</returns>
		public static T ReadFromStream<T>(this Stream file) where T : struct
		{
			var buff = new byte[Marshal.SizeOf(typeof(T))];
			file.Read(buff, 0, buff.Length);
			return GetStruct<T>(buff, 0);
		}

		/// <summary>
		/// Writes a struct to stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The object.</param>
		/// <param name="output">The output.</param>
		public static void WriteToStream<T>(this T obj, Stream output) where T : struct
        {
            var b = GetBytes(obj);

            output.Write(b, 0, b.Length);
        }

        /// <summary>
        /// Returns the size of a structure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.Int32.</returns>
        public static int SizeOf<T>() where T : struct
        {
            return Marshal.SizeOf(typeof(T));
        }
    }
}
