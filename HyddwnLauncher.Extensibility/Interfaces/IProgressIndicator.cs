namespace HyddwnLauncher.Extensibility.Interfaces
{
    /// <summary>
    ///     Represents a progress indicator
    /// </summary>
    public interface IProgressIndicator
    {
        /// <summary>
        ///     The percentage complete
        /// </summary>
        double ProgressBarPercent { get; set; }
        /// <summary>
        ///     Whether the progress bar should be indeterminate
        /// </summary>
        /// <param name="value"></param>
        void SetIsIndeterminate(bool value);
        /// <summary>
        ///     Sets the left text block
        /// </summary>
        /// <param name="text">Formatted text string</param>
        void SetLeftText(string text);
        /// <summary>
        ///     Sets the left text block
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void SetLeftText(string format, params object[] args);
        /// <summary>
        ///     Set the Progress bar Percentage
        /// </summary>
        /// <param name="value"></param>
        void SetProgressBar(double value);
        /// <summary>
        ///     Sets the right text block
        /// </summary>
        /// <param name="text">Formatted text string</param>
        void SetRightText(string text);
        /// <summary>
        ///     Sets the right text block
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void SetRightText(string format, params object[] args);
    }
}