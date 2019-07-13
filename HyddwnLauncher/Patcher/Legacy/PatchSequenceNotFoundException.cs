using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class PatchSequenceNotFoundException : Exception
    {
        public PatchSequenceNotFoundException(int bottom, int top)
            : base(string.Format(Properties.Resources.PatchSequenceNotFoundExceptionMessage, bottom, top))
        {
            Bottom = bottom;
            Top = top;
        }

        public int Bottom { get; protected set; }

        public int Top { get; protected set; }
    }
}
