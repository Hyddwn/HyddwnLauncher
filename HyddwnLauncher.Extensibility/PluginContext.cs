using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility
{
    public class PluginContext
    {
        public Action<string, bool> LogString;

        public Action<Exception, bool> LogException;

        public Action<string, string, double, bool, bool> MainUpdater;

        public Action<bool> SetPatcherState;

        public Action<IProgressIndicator> DestroyProgressIndicator;

        public Func<IPackEngine> GetPackEngine;

        public Func<IProgressIndicator> CreateProgressIndicator;

        public Func<INexonApi> GetNexonApi;
    }
}
