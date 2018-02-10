using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class LegacyPatcher
    {
        private readonly int _currentVersion;
        private INexonApi _nexonApi;
        private OfficialPatchInfo _officialPatchInfo;
        private PatchSequence _patchSequence;
        private int _remoteVersion;

        public LegacyPatcher()
        {
            _nexonApi = PatcherContext.Instance.PluginContext.GetNexonApi();
            _currentVersion = ReadVersion();
        }

        public async Task<bool> CheckForUpdates()
        {
            _officialPatchInfo = OfficialPatchInfo.Parse(MabiVersion.Versions
                .Find(version => version.Name == PatcherContext.Instance.ClientProfile.Localization).PatchUrl);

            if (int.TryParse(_officialPatchInfo["main_version"], out var versionConverted))
                _remoteVersion = versionConverted;

            if (_currentVersion == _remoteVersion) return false;

            try
            {
                _patchSequence = FindPatchSequence();
            }
            catch (PatchSequenceNotFoundException ex)
            {
                PatcherContext.Instance.PluginContext.LogException(ex, true);
                return false;
            }

            return true;
        }

        public async Task Update()
        {
            await Task.Run(() =>
            {
                var downloader = new PatchDownloader(_patchSequence, _officialPatchInfo);

                downloader.Prepare();

                downloader.Patch();
            });
        }

        private PatchSequence FindPatchSequence(int currentVersion = -1, int remoteVersion = -1)
        {
            if (currentVersion == -1)
                currentVersion = _currentVersion;
            if (remoteVersion == -1)
                remoteVersion = _remoteVersion;

            var patchInfos = new List<PatchInfo>();

            try
            {
                using (var webClient = new WebClient())
                {
                    if (currentVersion == 0)
                    {
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
                // TODO: UI Update happens here
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

        public static int ReadVersion()
        {
            try
            {
                return BitConverter.ToInt32(File.ReadAllBytes(
                    Path.GetDirectoryName(PatcherContext.Instance.ClientProfile.Location) + "version.dat"), 0);
            }
            catch
            {
                return 0;
            }
        }

        public static void WriteVersion(int version)
        {
            PatcherContext.Instance.PluginContext.LogString($"Writing {version} to version.dat");
            File.WriteAllBytes(Path.GetDirectoryName(PatcherContext.Instance.ClientProfile.Location) + "version.dat",
                BitConverter.GetBytes(version));
        }
    }
}