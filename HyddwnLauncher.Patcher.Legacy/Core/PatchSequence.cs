using System.Collections.Generic;
using System.Linq;
using HyddwnLauncher.Extensibility.Helpers;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class PatchSequence
    {
        public PatchSequence(IEnumerable<PatchInfo> patches)
        {
            Patches = new List<PatchInfo>(patches.OrderBy(p => p.StartVersion));
        }

        public int StartVersion => Patches.First().StartVersion;

        public int EndVersion => Patches.Last().EndVersion;

        public List<PatchInfo> Patches { get; protected set; }

        public long Size
        {
            get { return Patches.Sum(p => p.PatchSize); }
        }

        public override string ToString()
        {
            return
                $"Sequence from {StartVersion} to {EndVersion}. {Patches.Sum(p => p.Files.Count)} files. Total size: {ByteSizeHelper.ToString(Size)}";
        }
    }
}
