using System;

namespace HyddwnLauncher.Patching
{
    public class PatchSequenceNotFoundException : Exception
    {
        public PatchSequenceNotFoundException(int bottom, int top)
            : base("Cannot find a way to patch from " + bottom + " to " + top + "!")
        {
            Bottom = bottom;
            Top = top;
        }

        public int Bottom { get; protected set; }

        public int Top { get; protected set; }
    }
}