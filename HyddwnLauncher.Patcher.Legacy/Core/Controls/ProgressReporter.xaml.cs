namespace HyddwnLauncher.Patcher.Legacy.Core.Controls
{
    /// <summary>
    ///     Interaction logic for ProgressReporter.xaml
    /// </summary>
    public partial class ProgressReporter
    {
        public ProgressReporter(ProgressReporterViewModel progressReporterViewModel)
        {
            // This should just make the bindings automagically work!
            DataContext = progressReporterViewModel;
            InitializeComponent();
        }
    }
}