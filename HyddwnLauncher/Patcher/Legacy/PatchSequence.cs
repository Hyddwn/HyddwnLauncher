using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Util.Helpers;

namespace HyddwnLauncher.Patcher.Legacy
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
                string.Format("Sequence from {0} to {1}. {2} files. Total size: {3}", StartVersion, EndVersion,
                    Patches.Sum(p => p.Files.Count), ByteSizeHelper.ToString(Size));
        }
    }
}
