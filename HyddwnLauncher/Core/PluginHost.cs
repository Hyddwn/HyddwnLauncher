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
                Log.Warning(Properties.Resources.PluginDirectoryNotFound);
                try
                {
                    Directory.CreateDirectory(_pluginRoot);
                }
                catch (Exception ex)
                {
                    Log.Error(Properties.Resources.ExceptionCreatingPluginDirectory, ex.GetType().Name, ex.Message);
                }
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                Log.Exception(unauthorizedAccessException, Properties.Resources.UnableToLoadPlugins);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                MessageBox.Show(ex.Message, Properties.Resources.PluginLoadFailure);
            }

            if (Plugins == null) Plugins = new Collection<IPlugin>();
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
                    Log.Exception(ex, string.Format(Properties.Resources.ErrorShuttingDownPlugin, plugin.Name));
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
                    Log.Exception(ex, string.Format(Properties.Resources.ErrorCallingMethodInPlugin, "PatchBegin", plugin.Name));
                }
        }

        public void PatchEnd()
        {
            foreach (var plugin in Plugins)
                try
                {
                    plugin.PatchEnd();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, string.Format(Properties.Resources.ErrorCallingMethodInPlugin, "PatchEnd", plugin.Name));
                }
        }

        public void PreLaunch()
        {
            foreach (var plugin in Plugins) plugin.PreLaunch();
        }

        public void PostLaunch()
        {
            foreach (var plugin in Plugins) plugin.PostLaunch();
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
                    Log.Exception(ex, string.Format(Properties.Resources.ErrorSendingClientProfile, plugin.Name));
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
                    Log.Exception(ex, string.Format(Properties.Resources.ErrorSendingServerProfile, plugin.Name));
                }
        }
    }
}