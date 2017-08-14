using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility
{
    public interface IClientProfile
    {
        string Location { get; set; }
        string Name { get; set; }
    }
}
