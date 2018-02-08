using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility.Helpers;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class FileDownloader
    {
        public readonly ProgressReporterViewModel ProgressReporter;
        private readonly string _url;
        private readonly string _localFileName;
        private readonly double _fileSize;
        private CancellationToken _cancellationToken;
        
        private readonly Stopwatch _stopwatch;
        private long _totalRead;

        public FileDownloader(ProgressReporterViewModel progressReporterViewModel, string url, string localFileName, int fileSize, CancellationToken token = new CancellationToken())
        {
            ProgressReporter = progressReporterViewModel;
            _url = url;
            _localFileName = localFileName;
            _fileSize = fileSize;
            _cancellationToken = token;

            _stopwatch = new Stopwatch();
            _totalRead = 0;

            ProgressReporter.LeftText = localFileName;
        }

        public void Start()
        {
            PatcherContext.Instance.PluginContext.LogString($"Beginning doownload of {_url} to {_localFileName}...");
            using (var fileStream = new FileStream(_localFileName, FileMode.Create))
            using (var webClient = new WebClient())
            using (var stream = webClient.OpenRead(_url))
            {
                _stopwatch.Start();

                var buffer = new byte[4096];
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    fileStream.Write(buffer, 0, count);
                    _totalRead += count;
                    ProgressReporter.ProgressBarPercent = _totalRead / _fileSize * 100.0;
                    ProgressReporter.RightText = Math.Abs(_stopwatch.Elapsed.TotalSeconds) < double.Epsilon
                        ? ""
                        : $"{ByteSizeHelper.ToString(_totalRead)} - read at - {ByteSizeHelper.ToString(_totalRead / _stopwatch.Elapsed.TotalSeconds)}/s";
                }

                _stopwatch.Stop();
            }

            PatcherContext.Instance.PluginContext.LogString(
                $"Doownload of {_localFileName} complete, average speed of {ByteSizeHelper.ToString(_totalRead / _stopwatch.Elapsed.TotalSeconds)}/s");
        }

    }
}
