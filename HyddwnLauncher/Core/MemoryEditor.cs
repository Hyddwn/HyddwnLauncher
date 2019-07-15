using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Core
{
    public class MemoryEditor
    {
        //public List<MemoryPatch> MemoryPatches = new List<MemoryPatch>();
        private ClientProfile _clientProfile;
        private int _version;

        public MemoryEditor(ClientProfile clientProfile, int version)
        {
            _clientProfile = clientProfile;
            _version = version;
            
            //Before allowing more patches, I really want to store addresses because the scan is slooow
            //MemoryPatches.Add(new MemoryPatch
            //{
            //    Name = "Enable MultiClient",
            //    Patches = new List<PatchDefinition> { new PatchDefinition
            //    {
            //        Edit = new byte[] { 0x90, 0x90 },
            //        Offset = 0x07,
            //        Pattern = new short[] { 0xE8, -1, -1, -1, -1, 0x84, 0xC0, 0x74, -1, 0x8B, 0x0D, -1, -1, -1, -1, 0x8D, 0x45, 0xCC }
            //    } },
            //});
        }

        public void ApplyPatchesToProcessById(int processId)
        {
            Log.Info("Started patching process with ID: {0}", processId);
            var memory = new Memory(processId);
            var moduleHandle = memory.GetProcessModuleHandle("client.exe");

            if (moduleHandle == IntPtr.Zero)
                Log.Error("Failed to get module handle");

            var moduleInfo = new MODULEINFO();
            var success = Unmanaged.GetModuleInformation(memory.GetProcessHandle(), moduleHandle, out moduleInfo,
                (uint) Marshal.SizeOf(moduleInfo));

            if (!success)
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                Log.Error("Failed to get module info: {0}", error.Message);
            }

            Log.Info("Applying patch: Enable MultiClient");

            var pattern = new short[]
            {
                0xE8, -1, -1, -1, -1, 0x84, 0xC0, 0x74, -1, 0x8B, 0x0D, -1, -1, -1, -1, 0x8D,
                0x45, 0xCC
            };
            var offset = 0x07;
            var edit = new byte[] {0x90, 0x90};

            Log.Info("Pattern: {0}\r\nEdit Offset: {1}\r\nEdit: {2}", string.Join(", ", pattern), offset, string.Join(", ", edit));

            var address = IntPtr.Zero;

            if (_clientProfile.LastVersionForPatternSearch == _version)
            {
                Log.Info("Using last known address.");
                address = _clientProfile.LastAddressForPatterSearch;
            }

            if (address == IntPtr.Zero)
            {
                address = memory.QuickSearch((uint)moduleInfo.lpBaseOfDll,
                    (uint)moduleInfo.lpBaseOfDll + moduleInfo.SizeOfImage, pattern);

                _clientProfile.LastVersionForPatternSearch = _version;
                _clientProfile.LastAddressForPatterSearch = address;
            }

            memory.WriteProcMem(address + offset, edit);

            //foreach (var memoryPatch in MemoryPatches)
            //{
            //    Log.Info("Applying patch: {0}", memoryPatch.Name);

            //    foreach (var patch in memoryPatch.Patches)
            //    {
            //        Log.Info("Pattern: {0}\r\nEdit Offset: {1}\r\nEdit: {2}", string.Join(", ", patch.Pattern), patch.Offset, string.Join(", ", patch.Edit));
            //        var address = memory.QuickSearch((uint) moduleInfo.lpBaseOfDll,
            //            (uint) moduleInfo.lpBaseOfDll + moduleInfo.SizeOfImage, patch.Pattern);
            //        memory.WriteProcMem(address + patch.Offset, patch.Edit);
            //    }
            //}
        }
    }
}
