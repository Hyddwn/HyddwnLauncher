using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Network
{
    public static class AsyncDownloader
    {
        public static async Task DownloadWithCallbackAsync(string url, string file,
            Action<double, string> callback)
        {
            // Clears async warning, hack, need something better.
            await Task.Delay(1);

            var client = new WebClient();
            var sw = new Stopwatch();

            client.DownloadProgressChanged += (sender, args) =>
            {
                var bytesPerSecond = args.BytesReceived / sw.Elapsed.TotalSeconds;

                callback?.Raise(args.BytesReceived / (double) args.TotalBytesToReceive * 100,
                    $"{ByteSizeHelper.ToString(args.BytesReceived)}/{ByteSizeHelper.ToString(args.TotalBytesToReceive)} @ {ByteSizeHelper.ToString(bytesPerSecond, mode: ByteSizeMode.Network)}/s");
            };
            sw.Start();
            client.DownloadFileTaskAsync(url, file).Wait();
        }
    }
}