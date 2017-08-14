using System;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility
{
    public class PluginContext
    {
        public Func<INexonApi> GetNexonApi;

        public Func<IPackEngine> GetPackEngine;

        public Action<Exception, bool> LogException;
        public Action<string, bool> LogString;

        public Action<string, string, double, bool, bool> MainUpdater;

        public Action<bool> SetPatcherState;
    }
}