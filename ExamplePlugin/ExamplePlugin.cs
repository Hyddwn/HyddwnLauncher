using System.ComponentModel.Composition;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Primitives;

namespace ExamplePlugin
{
    [Export(typeof(IPlugin))]
    public class ExamplePlugin : PluginBase
    {
        public override void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile, IServerProfile activeServerProfile)
        {
            // Set the name of the plugin
            Name = "Example Plugin";

            // Call the base function (sets needed properties for you)
            base.Initialize(pluginContext, activeClientProfile, activeServerProfile);
        }
    }
}
