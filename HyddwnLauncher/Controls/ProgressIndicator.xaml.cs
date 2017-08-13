using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Core;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for ProgressIndicator.xaml
    /// </summary>
    public partial class ProgressIndicator : UserControl, IProgressIndicator
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
            Dispatcher.Invoke(() => ReporterProgressBar.SetMetroProgressSafe(value));
        }

        public void SetIsIndeterminate(bool value)
        {
            Dispatcher.Invoke(() => ReporterProgressBar.IsIndeterminate = value);
        }

        [StringFormatMethod("format")]
        public void SetLeftText(string format, params object[] args)
        {
            SetLeftText(string.Format(format, args));
        }

        public void SetLeftText(string text)
        {
            Dispatcher.Invoke(() => LeftTextBlock.Text = text);
        }

        [StringFormatMethod("format")]
        public void SetRightText(string format, params object[] args)
        {
            SetRightText(string.Format(format, args));
        }

        public void SetRightText(string text)
        {
            Dispatcher.Invoke(() => RighTextBlock.Text = text);
        }
    }
}