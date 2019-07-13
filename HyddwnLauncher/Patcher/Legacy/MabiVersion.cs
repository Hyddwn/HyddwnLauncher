using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class MabiVersion
    {
        public static readonly List<MabiVersion> Versions = new List<MabiVersion>(5)
        {
            new MabiVersion(ClientLocalization.Japan, "http://patch.mabinogi.jp/patch/patch.txt"),
            new MabiVersion(ClientLocalization.JapanHangame, "http://patch.mabinogi.jp/patch/patch_hangame.txt"),
            //new MabiVersion(ClientLocalization.Korea, "http://211.218.233.238/patch/patch.txt"),
            //new MabiVersion(ClientLocalization.KoreaTest, "http://211.218.233.238/patch/patch_test.txt"),
            //new MabiVersion(ClientLocalization.Taiwan, "http://tw.mabipatch.mabinogi.gamania.com/mabinogi/patch.txt")
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
            PatchUrl = url;
            SmartPatch = false;
        }

        public string Name { get; protected set; }
        public string PatchUrl { get; protected set; }
        public bool SmartPatch { get; protected set; }
    }
}
