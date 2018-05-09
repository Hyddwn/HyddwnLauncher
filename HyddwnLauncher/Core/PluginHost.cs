using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using HyddwnLauncher.Extensibility.Primitives;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Core
{
    public class PluginHost
    {
        private readonly string _pluginRoot =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins";

        public PluginHost()
        {
            try
            {
                var catalog = new DirectoryCatalog(_pluginRoot);
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (DirectoryNotFoundException)
            {
                Log.Warning($"Plugin directory was not found, creating.");
                try
                {
                    Directory.CreateDirectory(".\\Plugins");
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"Exception of type {ex.GetType().Name} occured when attempting to create the plugin directory: {ex.Message}");
                }
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                Log.Exception(unauthorizedAccessException, "Unable to load plugins.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                MessageBox.Show(ex.Message, "Plugin Load Failure");
            }
        }

        [ImportMany] public Collection<IPlugin> Plugins { get; protected set; }

        public void ShutdownPlugins()
        {
            foreach (var plugin in Plugins)
                try
                {
                    plugin.Shutdown();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"Error shutting down {plugin.Name}");
                }
        }

        public void PatchBegin()
        {
            foreach (var plugin in Plugins)
                try
                {
                    plugin.PatchBegin();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"An exception occured in {plugin.Name}'s PatchBegin method");
                }
        }

        public void PatchEnd()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.PatchEnd();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"An exception occured in {plugin.Name}'s PatchEnd method");
                }
            }
               
        }

        public void PreLaunch()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.PreLaunch();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"An exception occured in {plugin.Name}'s PreLaunch method");
                }
            }
        }

        public void PostLaunch()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.PostLaunch();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"An exception occured in {plugin.Name}'s PostLaunch method");
                }
            }
        }

        public void ClientProfileChanged(ClientProfile clientProfile)
        {
            foreach (var plugin in Plugins)
                try
                {
                    plugin.ClientProfileChanged(clientProfile);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"Error sending client profile update to {plugin.Name}");
                }
        }

        public void ServerProfileChanged(ServerProfile serverProfile)
        {
            foreach (var plugin in Plugins)
                try
                {
                    plugin.ServerProfileChanged(serverProfile);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"Error sending server profile update to {plugin.Name}");
                }
        }
    }
}