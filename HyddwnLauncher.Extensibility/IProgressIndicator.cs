namespace HyddwnLauncher.Extensibility
{
    public interface IProgressIndicator
    {
        double ProgressBarPercent { get; set; }
        void SetIsIndeterminate(bool value);
        void SetLeftText(string text);
        void SetLeftText(string format, params object[] args);
        void SetProgressBar(double value);
        void SetRightText(string text);
        void SetRightText(string format, params object[] args);
    }
}