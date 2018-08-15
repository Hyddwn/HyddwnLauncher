using System;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Primitives;

namespace HyddwnLauncher.Extensibility
{
    public abstract class PluginBase : IPlugin
    {
        protected internal IClientProfile ClientProfile;
        protected internal IServerProfile ServerProfile;
        protected internal PluginContext PluginContext;
        protected internal Guid Guid;

        public string Name { get; set; }

        public virtual void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
            IServerProfile activeServerProfile)
        {
            PluginContext = pluginContext;
            ClientProfile = activeClientProfile;
            ServerProfile = activeServerProfile;
            Guid = Guid.NewGuid();
        }

        public virtual Guid GetGuid()
        {
            return Guid;
        }

        public virtual void Shutdown()
        {
        }

        public virtual void ClientProfileChanged(IClientProfile clientProfile)
        {
            ClientProfile = clientProfile;
        }

        public virtual void ServerProfileChanged(IServerProfile serverProfile)
        {
            ServerProfile = serverProfile;
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

        public virtual void PatchBegin()
        {
        }

        public virtual void PatchEnd()
        {
        }
    }
}