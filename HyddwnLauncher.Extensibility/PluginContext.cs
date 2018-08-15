using System;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility
{
    /// <summary>
    ///     Used to make calls to Hyddwn Launcher to do specific tasks or retrive information.
    /// </summary>
    public class PluginContext
    {
        /// <summary>
        ///     Creates an instance of IPackEngine for use
        /// </summary>
        internal Func<IPackEngine> CreatePackEngineInternal;

        /// <summary>
        ///     Retrieves an instance of ISettingsManager
        /// </summary>
        internal Func<string, string, ISettingsManager> CreateSettingsManagerInternal;

        /// <summary>
        ///     Retrieve an instance of INexonApi for use
        /// </summary>
        internal Func<INexonApi> GetNexonApiInternal;

        /// <summary>
        ///     Gets the current patching state of the launcher
        /// </summary>
        internal Func<bool> GetPatcherStateInternal;

        /// <summary>
        ///     Exception: the exception to log
        ///     bool: show a messagebox with the exception
        /// </summary>
        internal Action<Exception, bool> LogExceptionInternal;

        /// <summary>
        ///     string: the message to log
        ///     bool: show a messagebox with the message
        /// </summary>
        internal Action<string, bool> LogStringInternal;

        /// <summary>
        ///     Updares the main progress reporter
        ///     string: left reporter text
        ///     string: right reporter text
        ///     double: progress bar value
        ///     bool: is indeterminate (don't know the progress)
        ///     bool: is progressbar visible
        /// </summary>
        internal Action<string, string, double, bool, bool> MainUpdaterInternal;

        /// <summary>
        ///     Makes the launcher prompt for their nexon user and password
        ///     Action: success action to invoke if the user logs in successfully
        ///     Action: cancel action to invoke if the user cancels the login
        /// </summary>
        internal Action<Action, Action> RequestUserLoginInternal;

        /// <summary>
        ///     Sets the active tab (only works for plugins)
        ///     Guid: the guid of the plugin
        /// </summary>
        internal Action<Guid> SetActiveTabInternal;

        /// <summary>
        ///     Sets wether the launcher should behaive as if it is patching
        /// </summary>
        internal Action<bool> SetPatcherStateInternal;

        /// <summary>
        ///     string: title
        ///     string: message
        ///     returns bool: success
        /// </summary>
        internal Func<string, string, bool> ShowDialogInternal;

        /// <summary>
        ///     Get the current patching state of the launcher
        /// </summary>
        /// <returns>Wether the patching state is active</returns>
        public bool GetPatcherState()
        {
            if (GetPatcherStateInternal != null)
                return GetPatcherStateInternal.Invoke();

            ThrowExceptionForUninitializedApiCall();
            return false;
        }

        /// <summary>
        ///     Retrieves an instance of INexonApi for use
        /// </summary>
        /// <returns>Interface reference to the NexonApi wrapper class instance</returns>
        public INexonApi GetNexonApi()
        {
            if (GetNexonApiInternal != null)
                return GetNexonApiInternal.Invoke();

            ThrowExceptionForUninitializedApiCall();
            return null;
        }

        /// <summary>
        ///     Creates an instnace of IPackEngine for use
        /// </summary>
        /// <returns></returns>
        public IPackEngine CreatePackEngine()
        {
            if (CreatePackEngineInternal != null)
                return CreatePackEngineInternal.Invoke();

            ThrowExceptionForUninitializedApiCall();
            return null;
        }

        // TODO: Change to allow different dialog types
        // TODO: Change return type
        /// <summary>
        ///     Shows a message dialog to the user with desired message and title
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Dialog message</param>
        /// <returns>Wether the user clicked affirmative</returns>
        public bool ShowDialog(string title, string message)
        {
            if (ShowDialogInternal != null)
                return ShowDialogInternal.Invoke(title, message);

            ThrowExceptionForUninitializedApiCall();
            return false;
        }

        /// <summary>
        ///     Logs an exception to the global log file
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="showMessagebox">Show a message box with the exception</param>
        public void LogException(Exception exception, bool showMessagebox)
        {
            if (LogExceptionInternal != null)
            {
                LogExceptionInternal.Invoke(exception, showMessagebox);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Logs a string to the global log file
        /// </summary>
        /// <param name="entry">The text to log</param>
        /// <param name="showMessagebox">Show a messagebox with the logged text</param>
        public void LogString(string entry, bool showMessagebox = false)
        {
            if (LogStringInternal != null)
            {
                LogStringInternal.Invoke(entry, showMessagebox);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Updates the main progress reporter
        /// </summary>
        /// <param name="leftText">The text that should appear on the left side of the reporter</param>
        /// <param name="rightText">The text that should appear on the right side of the reporter</param>
        /// <param name="progress">The value for the ProgressBar in the reporter</param>
        /// <param name="isIndeterminate">Set whether the ProgressBar is indeterminate or not</param>
        /// <param name="isProgressbarVisible">Set if the ProgressBar should be visible</param>
        public void UpdateMainProgress(string leftText = "", string rightText = "", double progress = 0,
            bool isIndeterminate = false, bool isProgressbarVisible = false)
        {
            if (MainUpdaterInternal != null)
            {
                MainUpdaterInternal.Invoke(leftText, rightText, progress, isIndeterminate, isProgressbarVisible);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Requests the user to log in with their NX Login
        /// </summary>
        /// <param name="successCallback">The callback that should be called if a login is successful</param>
        /// <param name="cancelCallback">The callback that should be called if a login is cancelled</param>
        public void RequestUserLogin(Action successCallback, Action cancelCallback)
        {
            if (RequestUserLoginInternal != null)
            {
                RequestUserLoginInternal.Invoke(successCallback, cancelCallback);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Sets the patching state for the launcher
        /// </summary>
        /// <param name="isEnabled">Whether the patching state should be active or not</param>
        public void SetPatcherState(bool isEnabled)
        {
            if (SetPatcherStateInternal != null)
            {
                SetPatcherStateInternal.Invoke(isEnabled);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Sets the active tab by plugin GUID
        /// </summary>
        /// <param name="guid">The GUID of the plugin tab to activate</param>
        public void SetActiveTab(string guid)
        {
            SetActiveTab(Guid.Parse(guid));
        }

        /// <summary>
        ///     Sets the active tab by plugin GUID
        /// </summary>
        /// <param name="guid">The GUID of the plugin tab to activate</param>
        public void SetActiveTab(Guid guid)
        {
            if (SetActiveTabInternal != null)
            {
                SetActiveTabInternal.Invoke(guid);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        private void ThrowExceptionForUninitializedApiCall([CallerMemberName] string methodName = null)
        {
            throw new ApplicationException($"{methodName ?? "??"} is not initialized for this plugin!");
        }
    }
}