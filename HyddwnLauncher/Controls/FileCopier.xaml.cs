using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Patching;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for FileCopier.xaml
    /// </summary>
    public partial class FileCopier : UserControl, IProgressReporter
    {
        public static readonly DependencyProperty LeftTextProperty = DependencyProperty.Register(
            "LeftText", typeof(string), typeof(FileCopier), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ProgressBarPercentProperty = DependencyProperty.Register(
            "ProgressBarPercent", typeof(double), typeof(FileCopier), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RightTextProperty = DependencyProperty.Register(
            "RightText", typeof(string), typeof(FileCopier), new PropertyMetadata(default(string)));

        private readonly string _dst;
        private readonly Stopwatch _s = new Stopwatch();
        private readonly string _src;
        private double _fSize;
        private CancellationToken _token;
        private long _totalRead;

        public FileCopier(string source, string destination, CancellationToken token)
        {
            _src = source;
            _dst = destination;
            _token = token;

            LeftText = Path.GetFileName(_src);
            InitializeComponent();
        }

        public string LeftText
        {
            get => (string) GetValue(LeftTextProperty);
            set => SetValue(LeftTextProperty, value);
        }

        public double ProgressBarPercent
        {
            get => (double) GetValue(ProgressBarPercentProperty);
            set => SetValue(ProgressBarPercentProperty, value);
        }

        public string RightText
        {
            get => (string) GetValue(RightTextProperty);
            set => SetValue(RightTextProperty, value);
        }


        public void Start()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dst));
            Log.Info("Copying from {0} to {1}", _src, _dst);
            using (var fileStream1 = new FileStream(_dst, FileMode.Create))
            {
                using (var fileStream2 = new FileStream(_src, FileMode.Open))
                {
                    _fSize = fileStream2.Length;
                    _s.Start();
                    var buffer = new byte[4096];
                    int count;
                    while ((count = fileStream2.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _token.ThrowIfCancellationRequested();
                        fileStream1.Write(buffer, 0, count);
                        _totalRead += count;
                        SetProgressBar(_totalRead / _fSize * 100.0);
                        SetRightText(Math.Abs(_s.Elapsed.TotalSeconds) < double.Epsilon
                            ? ""
                            : ByteSizeHelper.ToString(_totalRead) + " read at " +
                              ByteSizeHelper.ToString(_totalRead / _s.Elapsed.TotalSeconds) + "/s");
                    }
                    _s.Stop();
                }
            }
            Log.Info("Finished copying to {0}. Speed: {1}/s", _dst,
                ByteSizeHelper.ToString(_fSize / _s.Elapsed.TotalSeconds));
        }

        public void SetProgressBar(double value)
        {
            Dispatcher.Invoke(() => ProgressBarPercent = value);
        }

        public void SetRightText(string value)
        {
            Dispatcher.Invoke(() => RightText = value);
        }
    }
}