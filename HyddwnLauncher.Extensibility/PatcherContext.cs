using System;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.Extensibility
{
    /// <summary>
    ///     The API patchers use to report their progress
    /// </summary>
    public class PatcherContext
    {
        /// <summary>
        ///     Updates the main progress reporter
        ///     string: left reporter text
        ///     string: right reporter text
        ///     double: progress bar value
        ///     bool: is indeterminate (don't know the progress)
        ///     bool: is progressbar visible
        /// </summary>
        internal Action<string, string, double, bool, bool> MainUpdaterInternal;

        /// <summary>
        ///     Makes the launcher prompt for their Nexon user and password
        ///     Action: success action to invoke if the user logs in successfully
        ///     Action: cancel action to invoke if the user cancels the login
        /// </summary>
        internal Action<Action, Action> RequestUserLoginInternal;

        /// <summary>
        ///     Sets whether the launcher should behave as if it is patching
        /// </summary>
        internal Action<bool> SetPatcherStateInternal;

        /// <summary>
        ///     Changes the UI to show the progress of the patcher
        /// </summary>
        internal Action ShowSessionInternal;

        /// <summary>
        ///     Sets the UI to default state after patching
        /// </summary>
        internal Action HideSessionInternal;

        /// <summary>
        ///     Shows a MessageDialogModal
        ///     string: title
        ///     string: message
        ///     returns bool: success
        /// </summary>
        internal Func<string, string, bool> ShowDialogInternal;

        internal Func<IProgressIndicator> CreateProgressIndicatorInternal;

        internal Action<IProgressIndicator> DestroyProgressIndicatorInternal;

        internal Func<int> GetMaxDownloadWorkersInternal;

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
        ///     Changes the UI to show the progress of the patcher
        /// </summary>
        public void ShowSession()
        {
            if (ShowSessionInternal != null)
            {
                ShowSessionInternal.Invoke();
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Sets the UI to default state after patching
        /// </summary>
        public void HideSession()
        {
            if (HideSessionInternal != null)
            {
                HideSessionInternal.Invoke();
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Shows a message dialog to the user with desired message and title
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Dialog message</param>
        /// <returns>Whether the user clicked affirmative</returns>
        public bool ShowDialog(string title, string message)
        {
            if (ShowDialogInternal != null)
                return ShowDialogInternal.Invoke(title, message);

            ThrowExceptionForUninitializedApiCall();
            return false;
        }

        /// <summary>
        ///     Creates a ProgressIndicator on the UI thread
        /// </summary>
        /// <returns>WTHe instance of the created ProgressIndicator</returns>
        public IProgressIndicator CreateProgressIndicator()
        {
            if (CreateProgressIndicatorInternal != null)
                return CreateProgressIndicatorInternal.Invoke();

            ThrowExceptionForUninitializedApiCall();
            return null;
        }

        /// <summary>
        ///     Creates a ProgressIndicator on the UI thread
        /// </summary>
        /// <returns>The instance of the created ProgressIndicator</returns>
        public void DestroyProgressIndicator(IProgressIndicator progressIndicator)
        {
            if (DestroyProgressIndicatorInternal != null)
            {
                DestroyProgressIndicatorInternal.Invoke(progressIndicator);
                return;
            }

            ThrowExceptionForUninitializedApiCall();
        }

        /// <summary>
        ///     Get the max download worker count corresponding to LauncherSettings.ConnectionLimit />
        /// </summary>
        /// <returns>The max worker count</returns>
        public int GetMaxDownloadWorkers()
        {
            if (GetMaxDownloadWorkersInternal != null)
                return GetMaxDownloadWorkersInternal.Invoke();

            ThrowExceptionForUninitializedApiCall();
            return 10;
        }

        private void ThrowExceptionForUninitializedApiCall([CallerMemberName] string methodName = null)
        {
            throw new ApplicationException($"{methodName ?? "??"} is not initialized for this patcher!");
        }
    }
}
