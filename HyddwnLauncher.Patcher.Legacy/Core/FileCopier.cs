using System.Diagnostics;
using System.IO;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class FileCopier
    {
        private double _fileSize;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private long _totalRead;
        public ProgressReporterViewModel ProgressReporter;

        public FileCopier(ProgressReporterViewModel progressReporter)
        {
            ProgressReporter = progressReporter;
        }

        public void Start()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ProgressReporter.RightText));
            PatcherContext.Instance.PluginContext.LogString(
                $"Copying {ProgressReporter.LeftText} to {ProgressReporter.RightText}");

            using (var destinationFiileStream = new FileStream(ProgressReporter.RightText, FileMode.Create))
            {
                using (var sourceFileStream = new FileStream(ProgressReporter.LeftText, FileMode.Open))
                {
                    _fileSize = sourceFileStream.Length;
                    _stopwatch.Start();
                    var buffer = new byte[4096];
                    int count;
                    while ((count = sourceFileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationFiileStream.Write(buffer, 0, count);
                        _totalRead += count;
                        ProgressReporter.ProgressBarPercent = _totalRead / _fileSize * 100.0;
                    }

                    _stopwatch.Stop();
                }
            }

            PatcherContext.Instance.PluginContext.LogString($"Finished copying {ProgressReporter.LeftText}");
        }
    }
}