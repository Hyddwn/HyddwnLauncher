using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class LegacyPatcher : DefaultPatcher
    {
        private readonly int _currentVersion;
        private OfficialPatchInfo _officialPatchInfo;
        private PatchSequence _patchSequence;
        private int _remoteVersion;

        public LegacyPatcher(ClientProfile clientProfile, ServerProfile serverProfile, PatcherContext patcherContext) 
            : base(clientProfile, serverProfile, patcherContext)
        {
            PatcherType = DefaultPatcherTypes.Legacy;
            _currentVersion = ReadVersion();
        }

        public override async Task<bool> GetMaintenanceStatus()
        {
            if (_officialPatchInfo == null)
                await CheckForUpdates();

            return !_officialPatchInfo.PatchAccept;
        }

        public override async Task<string> GetLauncherArguments()
        {
            if (_officialPatchInfo == null)
                await CheckForUpdates();

            return $"code:1622 ver:{ReadVersion()} logip:{_officialPatchInfo["login"]} logport:11000 {_officialPatchInfo["arg"]}";
        }

        public override async Task<bool> CheckForUpdates()
        {
            if (ClientProfile == null) return false;
            if (ServerProfile == null) return false;

            // Should never hit, earlier logic shouldn't even call the function
            if (!ServerProfile.IsOfficial) return false;

            if (ClientProfile.Localization != ClientLocalization.Japan &&
                ClientProfile.Localization != ClientLocalization.JapanHangame)
                return false;

            PatcherContext.SetPatcherState(true);

            PatcherContext.UpdateMainProgress("Checking for updates...", "", 0, true, true);

            _officialPatchInfo = OfficialPatchInfo.Parse(MabiVersion.Versions
                .Find(version => version.Name == ClientProfile.Localization).PatchUrl);

            if (int.TryParse(_officialPatchInfo["main_version"], out var versionConverted))
                _remoteVersion = versionConverted;

            if (_currentVersion == _remoteVersion)
            {
                PatcherContext.SetPatcherState(false);
                PatcherContext.UpdateMainProgress("", "", 0, false, false);
                return false;
            }

            try
            {
                await Task.Run(() => _patchSequence = FindPatchSequence());
            }
            catch (PatchSequenceNotFoundException ex)
            {
                Log.Exception(ex);
                PatcherContext.SetPatcherState(false);
                PatcherContext.UpdateMainProgress("", "", 0, false, false);
                return false;
            }

            PatcherContext.SetPatcherState(false);
            PatcherContext.UpdateMainProgress("", "", 0, false, false);
            return true;
        }

        public override async Task<bool> ApplyUpdates()
        {
            PatcherContext.SetPatcherState(true);
            PatcherContext.ShowSession();

            var downloader = new PatchDownloader(PatcherContext, _patchSequence, _officialPatchInfo, ClientProfile);
            await Task.Run(() => downloader.Prepare());
            await Task.Run(() => downloader.Patch());
            WriteVersion(_remoteVersion);

            PatcherContext.SetPatcherState(false);
            PatcherContext.HideSession();
            return true;
        }

        private PatchSequence FindPatchSequence(int currentVersion = -1, int remoteVersion = -1)
        {
            if (currentVersion == -1)
                currentVersion = _currentVersion;
            if (remoteVersion == -1)
                remoteVersion = _remoteVersion;

            Log.Info("Attempting to find sequence from {0} to {1}", currentVersion, remoteVersion);
            var patchInfos = new List<PatchInfo>();
            var format = "Attempting to patch from {0} to {1}. Currently checking {2}.";
            try
            {
                using (var webClient = new WebClient())
                {
                    if (currentVersion == 0)
                    {
                        PatcherContext.UpdateMainProgress(string.Format(format, 0,
                            remoteVersion,
                            remoteVersion), "", 0, true, true);
                        var patchInfo = GetPatchInfo(webClient, remoteVersion, $"{remoteVersion}_full.txt");
                        if (patchInfo == null) throw new PatchSequenceNotFoundException(currentVersion, remoteVersion);
                        patchInfos.Add(patchInfo);
                    }
                    else
                    {
                        var fromNumber = currentVersion;
                        var toNumber = remoteVersion;

                        while (fromNumber != toNumber)
                        {
                            PatcherContext.UpdateMainProgress(string.Format(format, currentVersion,
                                remoteVersion,
                                fromNumber), "", (fromNumber - (double) currentVersion / (remoteVersion - currentVersion)), true, true);
                            var patchInfo = GetPatchInfo(webClient, remoteVersion, $"{fromNumber}_to_{toNumber}.txt");

                            if (patchInfo != null)
                            {
                                patchInfos.Add(patchInfo);
                                fromNumber = toNumber;
                                toNumber += 5;
                            }
                            else if (--toNumber == fromNumber)
                            {
                                return FindPatchSequence(0, remoteVersion);
                            }
                        }
                    }
                }
            }
            finally
            {
                PatcherContext.UpdateMainProgress();
            }

            return new PatchSequence(patchInfos);
        }

        private PatchInfo GetPatchInfo(WebClient webClient, int version, string filename)
        {
            var mainFtpCopy = _officialPatchInfo.MainFtp;

            if (!mainFtpCopy.EndsWith("/"))
                mainFtpCopy += "/";

            var address = $"{mainFtpCopy}{version}/{filename}";

            try
            {
                return new PatchInfo(Path.GetFileNameWithoutExtension(filename),
                    webClient.DownloadString(address));
            }
            catch
            {
                return null;
            }
        }

        public void OnClientProfileUpdated(ClientProfile clientProfile)
        {
            ClientProfile = clientProfile;
        }

        public void OnServerProfileUpdated(ServerProfile serverProfile)
        {
            ServerProfile = serverProfile;
        }
    }
}
