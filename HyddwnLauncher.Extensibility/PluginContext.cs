using System;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility
{
    /// <summary>
    /// Used to make calls to Hyddwn Launcher to do specific tasks or retrive information.
    /// </summary>
    public class PluginContext
    {
        /// <summary>
        /// Gets the current patching state of the launcher
        /// </summary>
        public Func<bool> GetPatcherState;

        /// <summary>
        /// Retrieve an instance of INexonApi for use
        /// </summary>
        public Func<INexonApi> GetNexonApi;

        /// <summary>
        /// Creates an instance of IPackEngine for use
        /// </summary>
        public Func<IPackEngine> GetPackEngine;

        /// <summary>
        /// string: title
        /// string: messages
        /// returns bool: success
        /// </summary>
        public Func<string, string, bool> ShowDialog;

        /// <summary>
        /// Exception: the exception to log
        /// bool: show a messagebox with the exception
        /// </summary>
        public Action<Exception, bool> LogException;

        /// <summary>
        /// string: the message to log
        /// bool: show a messagebox with the message
        /// </summary>
        public Action<string, bool> LogString;

        /// <summary>
        /// Updares the main progress reporter
        /// string: left reporter text
        /// string: right reporter text
        /// double: progress bar value
        /// bool: is indeterminate (don't know the progress)
        /// bool: is progressbar visible
        /// </summary>
        public Action<string, string, double, bool, bool> MainUpdater;

        /// <summary>
        /// Makes the launcher prompt for their nexon user and password
        /// Action: success action to invoke if the user logs in successfully
        /// Action: cancel action to invoke if the user cancels the login
        /// </summary>
        public Action<Action, Action> RequestUserLogin;

        /// <summary>
        /// Sets wether the launcher should behaive as if it is patching
        /// </summary>
        public Action<bool> SetPatcherState;

        /// <summary>
        /// Sets the active tab (only works for plugins)
        /// Guid: the guid of the plugin
        /// </summary>
        public Action<Guid> SetActiveTab;
    }
}