using System.Runtime.InteropServices;

namespace HyddwnLauncher.PackOps.Pack
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PackageItemInfo
    {
        public int Seed;
        public int Zero;
        [MarshalAs(UnmanagedType.U4)] public uint DataOffset;
        public int CompressedSize;
        public int DecompressedSize;
        [MarshalAs(UnmanagedType.Bool)] public bool IsCompressed;
        public long FileTime1;
        public long FileTime2;
        public long FileTime3;
        public long FileTime4;
        public long FileTime5;
    }
}