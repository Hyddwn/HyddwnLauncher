using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patching
{
    public class PatchInfo
    {
        public PatchInfo(string name, string info)
        {
            if (name.Contains("_to_"))
            {
                var strArray = name.Split(new string[1]
                {
                    "_to_"
                }, StringSplitOptions.None);
                StartVersion = int.Parse(strArray[0]);
                EndVersion = int.Parse(strArray[1]);
            }
            else
            {
                StartVersion = 0;
                EndVersion = int.Parse(new string(name.TakeWhile(c => c != 95).ToArray()));
            }
            Files = new List<PatchFileInfo>();
            using (var stringReader = new StringReader(info))
            {
                stringReader.ReadLine();
                while (stringReader.Peek() != -1)
                {
                    var str = stringReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        var strArray = str.Split(new string[1]
                        {
                            ", "
                        }, StringSplitOptions.None);
                        Files.Add(new PatchFileInfo(strArray[0], int.Parse(strArray[1]), strArray[2]));
                    }
                }
            }
        }

        public int StartVersion { get; protected set; }

        public int EndVersion { get; protected set; }

        public List<PatchFileInfo> Files { get; protected set; }

        public long PatchSize
        {
            get { return Files.Sum(f => (long) f.Size); }
        }

        public string PatchName
        {
            get
            {
                if (StartVersion != 0)
                    return StartVersion + "_to_" + EndVersion;
                return EndVersion + "_full";
            }
        }

        public string ZipFilePath => Path.Combine(PatchName, Path.ChangeExtension(PatchName, ".zip"));

        public string LanguageFilePath => Path.Combine(PatchName, "language.zip");

        public string ContentDirectory => Path.Combine(PatchName, "content");

        public string LanguageContentDir => Path.Combine(ContentDirectory, "package");

        public override string ToString()
        {
            return $"{PatchName}, {Files.Count} files, {ByteSizeHelper.ToString(PatchSize)}";
        }
    }
}