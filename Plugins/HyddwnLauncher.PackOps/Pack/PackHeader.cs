using System.Runtime.InteropServices;

namespace HyddwnLauncher.PackOps.Pack
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PackHeader
	{
        // 512 B
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public byte[/*8*/] Signature;
        public uint D1;
        public uint Sum;
		public long FileTime1;
		public long FileTime2;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 480)]
		public string/*char[480]*/ DataPath;

		// 32 B
		public uint FileCount;
		public uint HeaderLength;
		public uint BlankLength;
		public uint DataLength;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public byte[/*16*/] Zero;
	};
}
