using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using HyddwnLauncher.Patching;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Controls
{
    /// <summary>
    ///     Interaction logic for FileDownloader.xaml
    /// </summary>
    public partial class FileDownloader : IProgressReporter
    {
        public static readonly DependencyProperty LeftStringProperty = DependencyProperty.Register(
            "LeftText", typeof(string), typeof(FileDownloader), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ProgressBarPercentProperty = DependencyProperty.Register(
            "ProgressBarPercent", typeof(double), typeof(FileDownloader), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RightTextProperty = DependencyProperty.Register(
            "RightText", typeof(string), typeof(FileDownloader), new PropertyMetadata(default(string)));

        private readonly double _fSize;
        private readonly string _localFile;
        private readonly Stopwatch _s = new Stopwatch();
        private readonly string _url;
        private CancellationToken _token;
        private long _totalRead;

        public FileDownloader(string remote, string local, int size, CancellationToken token)
        {
            _url = remote;
            _localFile = local;
            _fSize = size;
            _token = token;

            InitializeComponent();
            LeftText = Path.GetFileName(_localFile);
        }

        public string LeftText
        {
            get => (string) GetValue(LeftStringProperty);
            set => SetValue(LeftStringProperty, value);
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
            Log.Info("Beginning download of {0} to {1}", _url, _localFile);
            using (var fileStream = new FileStream(_localFile, FileMode.Create))
            {
                using (var webClient = new WebClient())
                {
                    using (var stream = webClient.OpenRead(_url))
                    {
                        _s.Start();
                        var buffer = new byte[4096];
                        int count;
                        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _token.ThrowIfCancellationRequested();
                            fileStream.Write(buffer, 0, count);
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
            }
            Log.Info("Download of {0} complete, average speed {1}/s", _localFile,
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