namespace HyddwnLauncher.Patching
{
    public interface IProgressReporter
    {
        string LeftText { get; }

        double ProgressBarPercent { get; }

        string RightText { get; }

        void Start();
    }
}