using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Core
{
    [Flags]
    public enum MemoryProtectionConstants : uint
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000
    }

    [Flags]
    public enum ProcessAccessRights : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FFF,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        SYNCHRONIZE = 0x00100000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MODULEINFO
    {
        public IntPtr lpBaseOfDll;
        public uint SizeOfImage;
        public IntPtr EntryPoint;
    }

    public static class Unmanaged
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        /// <summary>
        /// Provided as consistency. Use System.Runtime.InteropServices.Marshal.GetLastWin32Error() instead.
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        public static IntPtr OpenProcess(ProcessAccessRights dwDesiredAccess, bool bInheritHandle, int dwProcessId) => OpenProcess((uint)dwDesiredAccess, bInheritHandle, dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, MemoryProtectionConstants flNewProtect, out MemoryProtectionConstants lpflOldProtect)
        {
            var ret = VirtualProtectEx(hProcess, lpAddress, dwSize, (uint)flNewProtect, out uint oldProtect);
            lpflOldProtect = (MemoryProtectionConstants)oldProtect;
            return ret;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, out uint lpcbNeeded);

        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        public static string TruncatePath(string path, int length)
        {
            StringBuilder sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }
    }
}
