using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Core
{
    public class PatchDefinition
    {
        public int Offset { get; set; }
        public byte[] Edit { get; set; }
        public short[] Pattern { get; set; }
    }
}
