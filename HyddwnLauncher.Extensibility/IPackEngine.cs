using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility
{
    public interface IPackEngine
    {
        void Pack(string inputDir, string outputFile, uint version, int level = 9);
    }
}
