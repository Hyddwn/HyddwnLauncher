using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HyddwnLauncher.Util;
using HyddwnLauncher.Util.Helpers;

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

        public async Task<string> ApplyPatchesToProcessByIdAsync(int processId)
        {
            Log.Info("Started patching process with ID: {0}", processId);
            var memory = new Memory(processId);

            await Task.Delay(500);

            Log.Info("Attempting to get module handle.");
            var moduleHandle = memory.GetProcessModuleHandle("client.exe");

            if (moduleHandle == IntPtr.Zero)
            {
                Log.Error("Patch Failed: Failed to get module handle");
                return "Patch failed! (Failed to get module handle)";
            }
                

            var moduleInfo = new MODULEINFO();
            var success = Unmanaged.GetModuleInformation(memory.GetProcessHandle(), moduleHandle, out moduleInfo,
                (uint) Marshal.SizeOf(moduleInfo));

            if (!success)
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                Log.Error("Patch Failed: Failed to get module info: {0}", error.Message);
                return string.Format("Patch Failed: Failed to get module info: {0}", error.Message);
            }

            Log.Info("Attempting to apply patch: Enable MultiClient");

            var pattern = new short[]
            {
                0xE8, -1, -1, 0x00, 0x00, 0x84, 0xC0, 0x74, -1, 0x8B, 0x0D, -1, -1, -1, -1, 0x8D, 0x45, 0xC0
            };

            var offset = 0x07;
            var edit = new byte[] {0x90, 0x90};

            Log.Info("Pattern: {0}\r\nEdit Offset: {1}\r\nEdit: {2}", PatternHelper.ConvertToPatternString(pattern), offset, PatternHelper.ConvertToPatternString(edit));

            var address = IntPtr.Zero;

            if (_clientProfile.LastVersionForPatternSearch == _version)
            {
                Log.Info("Using last known address.");
                address = _clientProfile.LastAddressForPatterSearch;
            }

            if (address == IntPtr.Zero)
            {
                address = await memory.QuickSearchUnprotectedAsync((uint)moduleInfo.lpBaseOfDll,
                    (uint)moduleInfo.lpBaseOfDll + moduleInfo.SizeOfImage, pattern);

                if (address == IntPtr.Zero)
                {
                    Log.Error("Patch Failed: No match for pattern.");
                    return "Patch Failed: No match for pattern.";
                }

                _clientProfile.LastVersionForPatternSearch = _version;
                _clientProfile.LastAddressForPatterSearch = address;
            }

            Log.Info("Start Address: {0:x8} | Address of Pattern Match: {1:x8}", (int)moduleInfo.lpBaseOfDll, (int)address);

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

            return null;
        }
    }
}
