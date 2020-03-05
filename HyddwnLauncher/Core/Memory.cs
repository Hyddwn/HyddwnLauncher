using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Core
{
    public class Memory
    {
        private IntPtr Proc { get; set; }

        public Memory() { }

        public Memory(int procId)
        {
            Proc = Unmanaged.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, procId);
            if (Proc == IntPtr.Zero)
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                if (error.ErrorCode == 5)
                    throw new Exception(string.Format("ERROR: OpenProcess error {0} try running as admin.", error.ErrorCode));
                throw new Exception(string.Format("ERROR: OpenProcess error {0}", error.Message));
            }
        }

        ~Memory()
        {
            Unmanaged.CloseHandle(Proc);
        }

        public IntPtr GetProcessHandle()
        {
            return Proc;
        }

        public IntPtr GetProcessModuleHandle(string moduleName)
        {
            var hMods = new IntPtr[1024];

            var gch = GCHandle.Alloc(hMods, GCHandleType.Pinned);
            var pModules = gch.AddrOfPinnedObject();

            var uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));

            if (Unmanaged.EnumProcessModules(Proc, pModules, uiSize, out uint cbNeeded) == 1)
            {
                var uiTotalNumberofModules = (int)(cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));

                for (var i = 0; i < uiTotalNumberofModules; ++i)
                {
                    var stringBuilder = new StringBuilder(1024);

                    Unmanaged.GetModuleFileNameEx(Proc, hMods[i], stringBuilder, (uint)(stringBuilder.Capacity));
                    if (Path.GetFileName(stringBuilder.ToString().ToLower()) == moduleName.ToLower())
                        return hMods[i];
                }
            }

            gch.Free();

            return IntPtr.Zero;
        }

        public byte[] ReadProcMem(int procId, IntPtr addressToRead, int lengthToRead)
        {
            if (Proc == IntPtr.Zero)
                Proc = Unmanaged.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, procId);

            return ReadProcMem(addressToRead, lengthToRead);
        }

        public byte[] ReadProcMem(IntPtr addressToRead, int lengthToRead)
        {
            var newProtect = MemoryProtectionConstants.PAGE_READWRITE;
            if (!Unmanaged.VirtualProtectEx(Proc, addressToRead, (UIntPtr) lengthToRead, newProtect,
                out MemoryProtectionConstants oldProtect))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: ReadProcMem set VirtualProtectEx error {0}", error.Message));
            }

            var buffer = new byte[lengthToRead];

            if (!Unmanaged.ReadProcessMemory(Proc, addressToRead, buffer, lengthToRead, out IntPtr bufferRead))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: ReadProcMem ReadProcessMemory error {0}", error.Message));
            }
            

            if (!Unmanaged.VirtualProtectEx(Proc, addressToRead, (UIntPtr)lengthToRead, oldProtect, out newProtect))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: ReadProcMem unset VirtualProtectEx error {0}", error.Message));
            }
            

            Unmanaged.FlushInstructionCache(Proc, addressToRead, (UIntPtr)lengthToRead);

            return buffer;
        }

        public int WriteProcMem(IntPtr procHandle, IntPtr addressToWrite, byte[] bytesToWrite)
        {
            if (Proc == IntPtr.Zero)
                Proc = Unmanaged.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, procHandle.ToInt32());

            return WriteProcMem(addressToWrite, bytesToWrite);
        }

        public int WriteProcMem(IntPtr addressToWrite, byte[] bytesToWrite)
        {
            var newProtect = MemoryProtectionConstants.PAGE_READWRITE;
            if (!Unmanaged.VirtualProtectEx(Proc, addressToWrite, (UIntPtr)bytesToWrite.Length, newProtect, out MemoryProtectionConstants oldProtect))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: WriteProcMem set VirtualProtectEx error {0}", error.Message));

            }
            
            if (!Unmanaged.WriteProcessMemory(Proc, addressToWrite, bytesToWrite, bytesToWrite.Length, out int bufferWritten))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: WriteProcMem WriteProcessMemory error {0}", error.Message));
            }

            if (!Unmanaged.VirtualProtectEx(Proc, addressToWrite, (UIntPtr)bytesToWrite.Length, oldProtect, out newProtect))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                throw new Exception(string.Format("ERROR: WriteProcMem unset VirtualProtectEx error {0}", error.Message));
            }

            Unmanaged.FlushInstructionCache(Proc, addressToWrite, (UIntPtr)bytesToWrite.Length);

            return bufferWritten;
        }

        public async Task<IntPtr> QuickSearch(uint lowerAddress, uint upperAddress, short[] searchPattern)
        {
            uint address = 0;

            await Task.Run(() =>
            {
                for (var i = lowerAddress; i < upperAddress; ++i)
                {
                    var found = true;
                    for (var x = 0; x < searchPattern.Length; ++x)
                    {
                        var read = ReadProcMem((IntPtr) i + x, 1);
                        if ((searchPattern[x] & 0xFF00) > 0)
                            continue;

                        if (read[0] == (searchPattern[x] & 0x00FF)) continue;
                        found = false;
                        break;
                    }

                    if (!found) continue;
                    address = i;
                    break;
                }
            });

            return (IntPtr)address;
        }
    }
}
