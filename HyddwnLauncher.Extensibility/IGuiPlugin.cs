using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyddwnLauncher.Extensibility
{
    public interface IGuiPlugin
    {
        // Usercontrol provided for this plugin if any
        UserControl GetPluginUi();
    }
}
