using System;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Primitives;

namespace HyddwnLauncher.Extensibility
{
    public abstract class PluginBase : IPlugin
    {
        public string Name { get; }

        public virtual void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
            IServerProfile activeServerProfile)
        {
        }

        public virtual Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public virtual void Shutdown()
        {
        }

        public virtual void ClientProfileChanged(IClientProfile clientProfile)
        {
        }

        public virtual void ServerProfileChanged(IServerProfile serverProfile)
        {
        }

        public virtual void PreLaunch()
        {
        }

        public virtual void PostLaunch()
        {
        }

        public virtual UserControl GetPluginUi()
        {
            return null;
        }
    }
}