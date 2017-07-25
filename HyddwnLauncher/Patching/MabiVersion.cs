using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HyddwnLauncher.Patching
{
    public class MabiVersion
    {
        public ObservableCollection<MabiVersion> ObservableVersions => new ObservableCollection<MabiVersion>(Versions);

        public static readonly List<MabiVersion> Versions = new List<MabiVersion>(10)
        {
            //new MabiVersion("Japan", "http://patch.mabinogi.jp/patch/patch.txt"),
            //new MabiVersion("Japan Hangame", "http://patch.mabinogi.jp/patch/patch_hangame.txt"),
            //new MabiVersion("Korea", "http://211.218.233.238/patch/patch.txt"),
            //new MabiVersion("Korea Test", "http://211.218.233.238/patch/patch_test.txt"),
            new MabiVersion("North America", "http://mabipatchinfo.nexon.net/patch/patch.txt")
            //new MabiVersion("Taiwan", "http://tw.mabipatch.mabinogi.gamania.com/mabinogi/patch.txt")
        };

        static MabiVersion()
        {
        }

        public MabiVersion()
        {
        }

        public MabiVersion(string name, string url)
        {
            Name = name;
            PatchInfoUrl = url;
            SmartPatch = false;
        }

        public string Name { get; protected set; }

        public string PatchInfoUrl { get; protected set; }

        public bool SmartPatch { get; protected set; }

        public OfficialPatchInfo GetPatchInfo()
        {
            var officialPatchInfo = OfficialPatchInfo.Parse(PatchInfoUrl);
            if (!Name.StartsWith("North America"))
                return officialPatchInfo;
            var startIndex = officialPatchInfo.MainFtp.LastIndexOf("/game", StringComparison.Ordinal);
            if (startIndex != -1)
                officialPatchInfo.MainFtp = officialPatchInfo.MainFtp.Remove(startIndex);
            return officialPatchInfo;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}