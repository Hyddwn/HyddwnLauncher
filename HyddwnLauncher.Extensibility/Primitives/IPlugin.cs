using System;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility.Primitives
{
    public interface IPlugin
    {
        /// <summary>
        ///     The name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The entry point for the plugin
        /// </summary>
        /// <param name="pluginContext"></param>
        /// <param name="activeClientProfile"></param>
        /// <param name="activeServerProfile"></param>
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

        /// <summary>
        ///     Handle when the client profile changes
        /// </summary>
        /// <param name="clientProfile"></param>
        void ClientProfileChanged(IClientProfile clientProfile);

        /// <summary>
        ///     Handle when the server profile changes
        /// </summary>
        /// <param name="serverProfile"></param>
        void ServerProfileChanged(IServerProfile serverProfile);

        /// <summary>
        ///     Handle actions that should be taken before launching the client
        /// </summary>
        void PreLaunch();

        /// <summary>
        ///     Handle actions that should be taken after launching the client
        /// </summary>
        void PostLaunch();

        /// <summary>
        ///     Called when the launcher it loading to populate a tab in the Launcher
        /// </summary>
        /// <returns></returns>
        UserControl GetPluginUi();

        /// <summary>
        ///     Actions to take before the patcher has started running 
        /// </summary>
        void PatchBegin();

        /// <summary>
        ///     Actions to take after the patcher has stopped
        /// </summary>
        // TODO: Pass the result back to a plugin?
        void PatchEnd();
    }
}