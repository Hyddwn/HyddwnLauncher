using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Core
{
    public class MemoryPatch
    {
        public string Name { get; set; }
        public List<PatchDefinition> Patches { get; set; }
    }
}
