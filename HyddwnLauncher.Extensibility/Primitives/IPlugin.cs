using System;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility.Primitives
{
    public interface IPlugin
    {
        // The name of this plugin
        string Name { get; }

        // The void Main for this plugin
        void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
            IServerProfile activeServerProfile);

        /// <summary>
        ///     Guid used to identify this application
        /// </summary>
        /// <returns></returns>
        Guid GetGuid();

        /// <summary>
        ///     Launcher is updating
        /// </summary>
        void Shutdown();

        void ClientProfileChanged(IClientProfile clientProfile);

        void ServerProfileChanged(IServerProfile serverProfile);

        void PreLaunch();

        void PostLaunch();

        UserControl GetPluginUi();
    }
}