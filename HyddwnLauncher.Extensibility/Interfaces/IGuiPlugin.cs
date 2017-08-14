using System.Windows.Controls;

namespace HyddwnLauncher.Extensibility.Interfaces
{
    public interface IGuiPlugin
    {
        // Usercontrol provided for this plugin if any
        UserControl GetPluginUi();
    }
}
