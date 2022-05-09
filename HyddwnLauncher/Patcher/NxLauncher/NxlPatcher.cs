using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Network;
using HyddwnLauncher.Network.Rest;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class NxlPatcher : DefaultPatcher
    {
        private int _latestVersion;
        private int _version;

        public NxlPatcher(IClientProfile clientProfile, IServerProfile serverProfile, PatcherContext patcherContext)
            : base(clientProfile, serverProfile, patcherContext)
        {
            PatcherType = DefaultPatcherTypes.NxLauncher;
            PatchSettingsManager.Initialize();
            PatchIgnore = new PatchIgnore(PatcherContext);
        }

        private dynamic PatchData { get; set; }
        internal List<FileDownloadInfo> FileDownloadInfos { get; private set; }
        internal PatchIgnore PatchIgnore { get; }
        public List<Patch> Patches { get; private set; }

        public override async Task<bool> CheckForUpdatesAsync()
        {
            if (ValidateAction(true)) return false;

            PatcherContext.UpdateMainProgress(Resources.Initialize, "", 0, true, true);

            bool? completed = null;

            PatcherContext.SetPatcherState(true);

            PatcherContext.UpdateMainProgress(Resources.CheckingForUpdates, "", 0, true, true);

            if (!NexonApi.Instance.IsAccessTokenValid(ClientProfile.Guid))
                PatcherContext.RequestUserLogin(() => completed = true, () => completed = false);

            while (!completed.HasValue) await Task.Delay(100);

            if (!completed.Value)
            {
                PatcherContext.SetPatcherState(false);
                PatcherContext.UpdateMainProgress();
                return false;
            }

            var result = await CheckForUpdatesInternalAsync(ReadVersion());

            PatcherContext.SetPatcherState(false);
            PatcherContext.UpdateMainProgress();

            return result;
        }

        public override async Task<bool> RepairInstallAsync()
        {
            var shouldUpdate = await CheckForUpdatesInternalAsync(-1, true);
            if (shouldUpdate) return await ApplyUpdatesAsync();
            PatcherContext.SetPatcherState(false);
            PatcherContext.UpdateMainProgress();
            return true;
        }

        private async Task<bool> CheckForUpdatesInternalAsync(int version = -1, bool overrideSettings = false)
        {
            if (ValidateAction()) return false;

            Patches = new List<Patch>();

            PatchIgnore.Initialize(ClientProfile.Location);

            _latestVersion = await NexonApi.Instance.GetLatestVersionAsync();

            _version = _latestVersion;

            await GetManifestJsonAsync(_version);

            _latestVersion = await GetManagedVersion();

            FileDownloadInfos = await GetFileDownloadInfoAsync();

            if (version < _latestVersion || overrideSettings ||
                (version == _latestVersion && PatchSettingsManager.Instance.PatcherSettings.ForceUpdateCheck))
            {
                GetPatchList(overrideSettings);

                return Patches.Count > 0;
            }

            if (version != _latestVersion)
                this.WriteVersion(_latestVersion);

            return false;
        }

        public override async Task<bool> ApplyUpdatesAsync()
        {
            if (ValidateAction()) return false;

            PatcherContext.SetPatcherState(true);
            PatcherContext.UpdateMainProgress(Resources.ApplyingUpdates, "", 0, true, true);

            var patchDownloader = new PatchDownloader(Patches, ClientProfile, PatcherContext);

            var result = false;

            try
            {
                patchDownloader.Prepare();
                result = await patchDownloader.PatchAsync();
                patchDownloader.Cleanup();
                if (result) WriteVersion(_latestVersion);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to patch!");
            }
            finally
            {
                PatcherContext.UpdateMainProgress(result ? Resources.PatchComplete : Resources.PatchFailed);

                PatcherContext.SetPatcherState(false);
                PatcherContext.HideSession();
            }

            return result;
        }

        public override async Task<string> GetLauncherArgumentsAsync()
        {
            var response = await NexonApi.Instance.GetLaunchConfig();
            var args = response.Arguments;
            var cla = new ClientLaunchArguments(args.ToArray());

            return cla.ToString();
        }

        public override async Task<bool> GetMaintenanceStatusAsync()
        {
            return await NexonApi.Instance.GetMaintenanceStatus();
        }

        private async Task<int> GetManagedVersion()
        {
            var restClient = new RestClient(new Uri("http://theproffessorslaboratory.net"), null);

            var request = restClient.Create("/api.php");

            var buildTime = Math.Round((double)PatchData.buildtime, 0, MidpointRounding.AwayFromZero);

            request = request
                .AddForm("Action", "CV")
                .AddForm("buildtime", buildTime.ToString(CultureInfo.InvariantCulture));

            var response = await request.ExecutePostAsync<string>();
            var data = await response.GetContentAsync();

            if (string.IsNullOrWhiteSpace(data)) return 0;

            if (data.StartsWith("-"))
            {
                data = data.Replace("-", string.Empty);
                Log.Info("New version registered: {0}", data);
            }

            return int.TryParse(data, out var version) ? version : 0;
        }

        private bool ValidateAction(bool silent = false)
        {
            if (ClientProfile == null)
            {
                if (!silent)
                    PatcherContext.ShowDialog(Resources.NoProfileSelected,
                        Resources.NoProfileSelectedMessage2);

                return true;
            }

            if (string.IsNullOrWhiteSpace(ClientProfile.Location))
            {
                if (!silent)
                    PatcherContext.ShowDialog(Resources.ProfileError,
                        string.Format(
                            Resources.ProfileErrorMessage,
                            ClientProfile.Name));
                return true;
            }

            if (!ServerProfile.IsOfficial)
            {
                if (!silent)
                    PatcherContext.ShowDialog(Resources.ActionCancelled,
                        Resources.ActionCancelledMessage);
                return true;
            }

            if (File.Exists("steam_connector_config.json"))
            {
                Log.Info("Patching for steam is not supported at this time.");
                return true;
            }

            if (ClientLocalization.GetLocalization(ClientProfile.Localization) != "NorthAmerica")
            {
                if (!silent)
                    PatcherContext.ShowDialog(Resources.UnsupportedLocalization,
                        Resources.UnsupportedLocalizationMessage);
                return true;
            }

            return false;
        }

        private async Task GetManifestJsonAsync(int version)
        {
            if (version == -1)
            {
                PatcherContext.UpdateMainProgress(Resources.GettingLatestVersion, "", 0, true, true);
                _version = version = await NexonApi.Instance.GetLatestVersionAsync();
            }

            PatcherContext.UpdateMainProgress(Resources.DownloadingManifest, "", 0, true, true);
            // Get Manifest String
            var manifestHashString = await NexonApi.Instance.GetManifestHashStringAsync();

            // Download Manifest
            var buffer = DownloadManifestBuffer(manifestHashString);

            PatcherContext.UpdateMainProgress(Resources.DecompressingManifest, "", 0, true, true);

            // Decompress Manifest and Convert to Object
            var manifestContent = ZlibStream.UncompressString(buffer);
            var data = JsonConvert.DeserializeObject<dynamic>(manifestContent);

            PatchData = data;
        }

        private async Task<List<FileDownloadInfo>> GetFileDownloadInfoAsync()
        {
            double entries = ((ICollection) PatchData.files).Count;

            PatcherContext.UpdateMainProgress(Resources.ParsingManifest, $"0/{entries}", 0, false, true);
            var fileDownloadInfos = new List<FileDownloadInfo>();

            await Task.Delay(1);

            var entry = 0;

            // Decode filepaths into new collection
            foreach (var file in PatchData["files"])
            {
                entry++;
                PatcherContext.UpdateMainProgress(Resources.ParsingManifest, $"{entry}/{entries}",
                    entry / entries * 100, false, true);

                var isDirectory = false;
                var filename = file.Name;
                var decodedFilename = DecodeFilename(filename);

                // Nexon does not intend for this file to be downloaded or this may be some other specification
                if (file.Value["objects"].Count == 0)
                {
                    Log.Info(Resources.FileWithNoObjectsIgnored, decodedFilename);
                    continue;
                }

                if (file.Value["objects"][0] == "__DIR__")
                    isDirectory = true;

                var fileDownloadInfo = new FileDownloadInfo(decodedFilename, file.Value["fsize"].Value,
                    isDirectory ? FileInfoType.Directory : FileInfoType.File);

                fileDownloadInfo.SetModifiedTimeDateTimeUtc(file.Value["mtime"].Value);

                if (isDirectory)
                    continue;

                for (var i = 0; i < file.Value["objects"].Count; i++)
                {
                    var filePartInfo =
                        new FilePartInfo(file.Value["objects"][i].Value, decodedFilename,
                            file.Value["objects_fsize"][i].Value, i);
                    fileDownloadInfo.FileParts.Add(filePartInfo);
                }

                fileDownloadInfos.Add(fileDownloadInfo);
            }

            return fileDownloadInfos;
        }

        public bool GetIsNewPackFile(string filePath)
        {
            return GetIsNewPackFile(filePath, _version, _latestVersion);
        }

        public bool GetIsNewPackFile(string filePath, int minimum, int maximum)
        {
            if (string.IsNullOrWhiteSpace(filePath) || minimum >= maximum)
                return false;

            var packName = Path.GetFileName(filePath);

            string match;
            var packVersion = -1;

            if (packName.Contains("_to_"))
            {
                const string matchRegex = @"(_\d+)";
                match = Regex.Match(packName, matchRegex).Value;
                var toMatch = match.Replace("_", "");

                int.TryParse(toMatch, out packVersion);
            }

            if (packName.Contains("full"))
            {
                const string matchRegex = @"(\d+_)";
                match = Regex.Match(packName, matchRegex).Value;
                var fromMatch = match.Replace("_", "");

                int.TryParse(fromMatch, out packVersion);
            }

            if (packVersion != -1) return packVersion.IsWithin(minimum + 1, maximum);

            Log.Warning(
                "There was an issue with detecting the pack version using the pack naming scheme. Forcing the download of '{0}'",
                filePath);
            return true;
        }

        private static string DecodeFilename(string filename)
        {
            var filenameDecodedBytes = Convert.FromBase64String(filename);
            var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, filenameDecodedBytes);
            var chars = new char[utf8Bytes.Length];

            for (var i = 0; i < utf8Bytes.Length; i++) chars[i] = BitConverter.ToChar(new byte[] {utf8Bytes[i], 0}, 0);

            // Return UTF8
            return new string(chars).Substring(3);
        }


        private static byte[] DownloadManifestBuffer(string manifestHashString)
        {
            using (var client = new WebClient())
            using (var stream =
                   client.OpenRead($"http://download2.nexon.net/Game/nxl/games/10200/{manifestHashString}"))
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

            PatcherContext.UpdateMainProgress(Resources.CheckingFiles, $"0/{entries}", 0, false, true);

            var entry = 0;

            foreach (var fileDownloadInfo in FileDownloadInfos)
            {
                entry++;
                PatcherContext.UpdateMainProgress(Resources.CheckingFiles, $"{entry}/{entries}",
                    entry / entries * 100, false,
                    true);

                var filePath = fileDownloadInfo.FileName;

                //if (!overrideSettings)
                //    if (filePath.StartsWith("package\\")
                //        && PatchSettingsManager.Instance.PatcherSettings.IgnorePackageFolder
                //        && !GetIsNewPackFile(filePath))
                //        continue;

                if (PatchIgnore.IgnoredFiles.Contains(filePath))
                {
                    Log.Info($"File: '{filePath}' in ignore list, file will not be patched!");
                    continue;
                }

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
                    actualModified = File.GetLastWriteTimeUtc(filePath);
                    if (actualModified != fileDownloadInfo.LastModifiedDateTime)
                        modified = true;
                    else if (length != fileDownloadInfo.FileSize)
                        correctSize = false;
                }
                else
                {
                    fileExists = false;
                }

                if (!correctSize)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.SizeNotMatch);
                    Patches.Add(patch);
                    Log.Info(Resources.PatchRequiredForSize, filePath,
                        patch.PatchReason.LocalizedPatchReason(), fileDownloadInfo.FileSize, length);
                    continue;
                }

                if (!fileExists)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.DoesNotExist);
                    Patches.Add(patch);
                    Log.Info(Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                    continue;
                }

                if (!modified) continue;

                if (actualModified > fileDownloadInfo.LastModifiedDateTime)
                {
                    var patch = new Patch(fileDownloadInfo, PatchReason.Modified);
                    Patches.Add(patch);
                    Log.Info(Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                }
                else
                {
                    var patch = new Patch(fileDownloadInfo);
                    Patches.Add(patch);
                    Log.Info(Resources.PatchRequiredFor, filePath, patch.PatchReason.LocalizedPatchReason());
                }
            }
        }
    }
}