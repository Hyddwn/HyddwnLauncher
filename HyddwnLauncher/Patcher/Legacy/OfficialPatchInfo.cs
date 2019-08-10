using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class OfficialPatchInfo
    {
        private static readonly Regex AuthRegex = new Regex("(://)?(([^:]+):([^@]+)@)");
        private readonly Dictionary<string, string> _info = new Dictionary<string, string>();
        public string MainFtp;
        public int MainVersion;
        public bool PatchAccept;

        public string this[string index]
        {
            get
            {
                if (!_info.ContainsKey(index))
                    return null;
                return _info[index];
            }
            protected set => _info[index] = value;
        }

        public static OfficialPatchInfo OfflineJapanPatchInfo
        {
            get
            {
                var opi = new OfficialPatchInfo();

                var i = 441;
                var v = i.ToString();
                var f = "webdown2.nexon.co.jp:80/mabinogi/patch";

                opi["patch_accept"] = "0";
                opi["local_version"] = v;
                opi["local_ftp"] = f;
                opi["main_version"] = v;
                opi["main_ftp"] = f;
                opi["launcherinfo"] = "189";
                opi["login"] = "52.193.112.90";
                opi["arg"] = "chatip:18.182.225.218 chatport:8002 setting:\"file://data/features.xml=Regular, Japan\"";
                opi["lang"] = "patch_langpack.txt";

                opi.MainFtp = f;
                opi.MainVersion = i;
                opi.PatchAccept = false;

                return opi;
            }
        }

        public static OfficialPatchInfo OfflineJapanHangamePatchInfo
        {
            get
            {
                var opi = new OfficialPatchInfo();

                var i = 441;
                var v = i.ToString();
                var f = "webdown2.nexon.co.jp:80/mabinogi/patch_hangame";

                opi["patch_accept"] = "0";
                opi["local_version"] = v;
                opi["local_ftp"] = f;
                opi["main_version"] = v;
                opi["main_ftp"] = f;
                opi["launcherinfo"] = "189";
                opi["login"] = "52.193.112.90";
                opi["arg"] = "chatip:18.182.225.218 chatport:8002 setting:\"file://data/features.xml=Regular, Japan\" sublocale:nhnjapan";
                opi["lang"] = "patch_langpack.txt";

                opi.MainFtp = f;
                opi.MainVersion = i;
                opi.PatchAccept = false;

                return opi;
            }
        }

        public static OfficialPatchInfo Parse(string url)
        {
            var opi = new OfficialPatchInfo();
            Log.Info($"Downloading official patch information from {url}");
            using (var webClient = new WebClient())
            {
                var s = webClient.DownloadString(url);
                Log.Info($"Official Patch Info:\r\n{s}");
                using (var stringReader = new StringReader(s))
                {
                    while (stringReader.Peek() != -1)
                    {
                        var str = stringReader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(str) && str.Contains('='))
                        {
                            var strArray = str.Split(new char[1]
                            {
                                '='
                            }, 2);
                            opi._info[strArray[0]] = strArray[1];
                        }
                    }
                }
            }

            opi.PatchAccept = Convert.ToBoolean(int.Parse(opi["patch_accept"]));
            opi.MainVersion = int.Parse(opi["main_version"]);
            opi.MainFtp = ParseFtp(opi, "main_ftp");
            return opi;
        }

        private static string ParseFtp(OfficialPatchInfo opi, string id)
        {
            var input = opi[id];
            var match = AuthRegex.Match(input);
            if (match.Success)
            {
                opi["username"] = match.Groups[3].Value;
                opi["password"] = match.Groups[4].Value;
            }

            if (!input.Contains("://"))
                input = (input.Contains(":80") ? "http://" : "ftp://") + input;
            opi[id] = input;
            return input;
        }
    }
}
