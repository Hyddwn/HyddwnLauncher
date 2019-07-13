using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using HyddwnLauncher.Controls;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class NxlPatcher : DefaultPatcher
    {
        private dynamic PatchData { get; set; }
        internal List<FileDownloadInfo> FileDownloadInfos { get; private set; }
        public List<Patch> Patches { get; private set; }
        private int _version;
        private bool _possibleNewPack;

        public NxlPatcher(IClientProfile clientProfile, IServerProfile serverProfile, PatcherContext patcherContext)
            : base(clientProfile, serverProfile, patcherContext)
        {
            PatcherType = DefaultPatcherTypes.NxLauncher;
            PatchSettingsManager.Initialize();
        }

        public override async Task<bool> CheckForUpdates()
        {
            PatcherContext.UpdateMainProgress(Properties.Resources.Initialize, "", 0, true, true);

            var completed = "true";

            PatcherContext.SetPatcherState(true);

            PatcherContext.UpdateMainProgress(Properties.Resources.CheckingForUpdates, "", 0, true, true);

            if (!NexonApi.Instance.IsAccessTokenValid(ClientProfile.Guid))
            {
                completed = "";
                PatcherContext.RequestUserLogin(() => completed = "true", () => completed = "false");
            }

            while (completed == "")
            {
                await Task.Delay(100);
            }

            if (completed == "false")
            {
                PatcherContext.SetPatcherState(false);
                PatcherContext.UpdateMainProgress("", "", 0, false, false);
                return false;
            }

            var result = await CheckForUpdatesInternal();

            PatcherContext.SetPatcherState(false);
            PatcherContext.UpdateMainProgress("", "", 0, false, false);
            return result;
        }

        private async Task<bool> CheckForUpdatesInternal()
        {
            var version = -1;

            Patches = new List<Patch>();

            var latestVersion = await NexonApi.Instance.GetLatestVersion();

            _possibleNewPack = version < latestVersion;

            _version = latestVersion;

            await GetManifestJson(_version);

            FileDownloadInfos = await GetFileDownloadInfo();

            if (version >= latestVersion && (version < latestVersion || !PatchSettingsManager.Instance.PatcherSettings.ForceUpdateCheck)) return false;

            GetPatchList();

            return Patches.Count > 0;
        }

        public override async Task<bool> ApplyUpdates()
        {
            if (ValidateAction()) return false;

            PatcherContext.SetPatcherState(true);
            PatcherContext.UpdateMainProgress(Properties.Resources.ApplyingUpdates, "", 0, true, true);

            var patchDownloader = new PatchDownloader(Patches, ClientProfile, PatcherContext);
            await Task.Run(() => patchDownloader.Prepare());
            var result = await Task.Run(() => patchDownloader.Patch());
            await Task.Run(() => patchDownloader.Cleanup());
            PatcherContext.UpdateMainProgress(result ? Properties.Resources.PatchComplete : Properties.Resources.PatchFailed);

            PatcherContext.SetPatcherState(false);

            return result;
        }

        public override async Task<string> GetLauncherArguments()
        {
            var response = await NexonApi.Instance.GetLaunchConfig();
            var args = response.LaunchConfig.Arguments;
            var argString = "";
            foreach (var arg in args)
            {
                if (arg.StartsWith("-NXAL"))
                    continue;
                argString += arg + " ";
            }

            return argString;
        }

        public override async Task<bool> GetMaintenanceStatus()
        {
            return await NexonApi.Instance.GetMaintenanceStatus();
        }

        private bool ValidateAction()
        {
            if (ClientProfile == null)
            {
                {
                    PatcherContext.ShowDialog(Properties.Resources.NoProfileSelected,
                        Properties.Resources.NoProfileSelectedMessage2);
                }
                return true;
            }

            if (string.IsNullOrWhiteSpace(ClientProfile.Location))
            {
                PatcherContext.ShowDialog(Properties.Resources.ProfileError,
                    string.Format(
                        Properties.Resources.ProfileErrorMessage,
                        ClientProfile.Name));
                return true;
            }

            if (!ServerProfile.IsOfficial)
            {
                PatcherContext.ShowDialog(Properties.Resources.ActionCancelled,
                    Properties.Resources.ActionCancelledMessage);
                return true;
            }

            if (ClientLocalization.GetLocalization(ClientProfile.Localization) != "NorthAmerica")
            {
                PatcherContext.ShowDialog(Properties.Resources.UnsupportedLocalization,
                    Properties.Resources.UnsupportedLocalizationMessage);
                return true;
            }

            return false;
        }

        private async Task GetManifestJson(int version)
        {
            if (version == -1)
            {
                PatcherContext.UpdateMainProgress(Properties.Resources.GettingLatestVersion, "", 0, true, true);
                _version = version = await NexonApi.Instance.GetLatestVersion();
            }

            PatcherContext.UpdateMainProgress(Properties.Resources.DownloadingManifest, "", 0, true, true);
            // Get Manifest String
            var manifestHashString = await NexonApi.Instance.GetManifestHashString();

            // Download Manifest
            var buffer = DownloadManifestBuffer(manifestHashString);

            PatcherContext.UpdateMainProgress(Properties.Resources.DecompressingManifest, "", 0, true, true);

            // Decompress Manifest and Convert to Object
            var manifestContent = ZlibStream.UncompressString(buffer);
            var data = JsonConvert.DeserializeObject<dynamic>(manifestContent);

            PatchData = data;
        }

        private async Task<List<FileDownloadInfo>> GetFileDownloadInfo()
        {
            double entries = ((ICollection)PatchData.files).Count;

            PatcherContext.UpdateMainProgress(Properties.Resources.ParsingManifest, $"0/{entries}", 0, false, true);
            var fileDownloadInfos = new List<FileDownloadInfo>();

            await Task.Delay(1);

            int entry = 0;

            // Decode filepaths into new collection
            foreach (var file in PatchData["files"])
            {
                entry++;
                PatcherContext.UpdateMainProgress(Properties.Resources.ParsingManifest, $"{entry}/{entries}", entry / entries * 100, false, true);

                var isDirectory = false;
                var filename = file.Name;
                var decodedFilename = DecodeFilename(filename);

                // Nexon does not intend for this file to be downloaded or this may be some other specification
                if (file.Value["objects"].Count == 0)
                {
                    Log.Info(Properties.Resources.FileWithNoObjectsIgnored,  decodedFilename);
                    continue;
                }

                if (file.Value["objects"][0] == "__DIR__")
                    isDirectory = true;

                var fileDownloadInfo = new FileDownloadInfo(decodedFilename, file.Value["fsize"].Value,
                    isDirectory ? FileInfoType.Directory : FileInfoType.File);

                fileDownloadInfo.SetModifiedTimeDateTime(file.Value["mtime"].Value);

                if (isDirectory)
                    continue;

                for (var i = 0; i < file.Value["objects"].Count; i++)
                {
                    var filePartInfo =
                        new FilePartInfo(file.Value["objects"][i].Value, decodedFilename, file.Value["objects_fsize"][i].Value, i);
                    fileDownloadInfo.FileParts.Add(filePartInfo);
                }

                fileDownloadInfos.Add(fileDownloadInfo);
            }

            return fileDownloadInfos;
        }

        private static string DecodeFilename(string filename)
        {
            var filenameDecodedBytes = Convert.FromBase64String(filename);
            var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, filenameDecodedBytes);
            var chars = new char[utf8Bytes.Length];

            for (var i = 0; i < utf8Bytes.Length; i++)
            {
                chars[i] = BitConverter.ToChar(new byte[] { utf8Bytes[i], 0 }, 0);
            }

            // Return UTF8
            return new string(chars).Substring(3);
        }

        private static byte[] DownloadManifestBuffer(string manifestHashString)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead($"https://download2.nexon.net/Game/nxl/games/10200/{manifestHashString}"))
            using (var ms = new MemoryStream())
            {
                stream?.CopyTo(ms);
                var buffer = ms.ToArray();
                return buffer;
            }
        }

        private void GetPatchList(bool overrideSettings = false)
        {
            double entries = FileDownloadInfos.Count;

            PatcherContext.UpdateMainProgress(Properties.Resources.CheckingFiles, $"0/{entries}", 0, false, true);

            var entry = 0;

            foreach (var fileDownloadInfo in FileDownloadInfos)
            {
                entry++;
                PatcherContext.UpdateMainProgress(Properties.Resources.CheckingFiles, $"{entry}/{entries}", entry / entries * 100, false,
                    true);

                var filePath = fileDownloadInfo.FileName;

            if (!overrideSettings)
                if (filePath.StartsWith("package\\")
                    && PatchSettingsManager.Instance.PatcherSettings.IgnorePackageFolder
                    && (_possibleNewPack && !filePath.Contains(_version.ToString()) || !_possibleNewPack)) continue;


                if (fileDownloadInfo.FileInfoType == FileInfoType.Directory)
                {
                    if (Directory.Exists(fileDownloadInfo.FileName)) continue;

                    Patches.Add(new Patch(fileDownloadInfo, PatchReason.DoesNotExist));
                    continue;
                }

                var modified = false;
                var fileExists = true;
                var actualModified = new DateTime();
                var correctSize = true;
                long length = 0;

                if (File.Exists(filePath))
                {
                    length = new FileInfo(filePath).Length;
                    actualModified = File.GetLastWriteTime(filePath);
                    if (actualModified != fileDownloadInfo.LastModifiedDateTime)
                        modified = true;
                    else if (length != fileDownloadInfo.FileSize)
                        correctSize = false;
                }
                else
                    fileExists = false;

                if (!correctSize)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.SizeNotMatch);
                    Patches.Add(patch);
                    Log.Info(Properties.Resources.PatchRequiredForSize, filePath, patch.PatchReason.LocalizedPatchReason(), fileDownloadInfo.FileSize, length);
                    continue;
                }

                if (!fileExists)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.DoesNotExist);
                    Patches.Add(patch);
                    Log.Info(Properties.Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                    continue;
                }

                if (!modified) continue;

                if (actualModified > fileDownloadInfo.LastModifiedDateTime)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.Modified);
                    Patches.Add(patch);
                    Log.Info(Properties.Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                }
                else
                {
                    var patch = new Patch(fileDownloadInfo);
                    Patches.Add(patch);
                    Log.Info(Properties.Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                }
            }
        }
    }
}
