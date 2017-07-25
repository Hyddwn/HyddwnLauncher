using System.Windows;
using System.Windows.Controls;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for ProgressIndicator.xaml
    /// </summary>
    public partial class ProgressIndicator : UserControl
    {
        public static readonly DependencyProperty ProgressBarPercentProperty = DependencyProperty.Register(
            nameof(ProgressBarPercent), typeof(double), typeof(ProgressIndicator),
            new PropertyMetadata(default(double)));

        public ProgressIndicator()
        {
            InitializeComponent();
        }

        public double ProgressBarPercent
        {
            get => (double) GetValue(ProgressBarPercentProperty);
            set => SetValue(ProgressBarPercentProperty, value);
        }

        public void SetProgressBar(double value)
        {
            Dispatcher.Invoke(() => ProgressBarPercent = value);
        }
    }
}