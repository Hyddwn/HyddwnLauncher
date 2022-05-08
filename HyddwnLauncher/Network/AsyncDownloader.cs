using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.UI;
using HyddwnLauncher.Util;
using HyddwnLauncher.Util.Helpers;

namespace HyddwnLauncher.Network
{
    public static class AsyncDownloader
    {
        public static async Task DownloadFileWithCallbackAsync(string url, string file,
            Action<double, string> callback, bool specialOperation = false, int updateReduction = 1)
        {
            await Task.Delay(1);
            var client = new WebClient();
            var sw = new Stopwatch();
            var updateCounter = 0;

            if (specialOperation)
            {
                client.Proxy = new WebProxy();
                client.Headers.Add("Accept-Encoding", "identity");
                client.Headers.Add("Pragma", "akamai-x-cache-on");
            }

            client.DownloadProgressChanged += (sender, args) =>
            {
                var bytesPerSecond = args.BytesReceived / sw.Elapsed.TotalSeconds;

                updateCounter++;

                if (updateCounter % updateReduction == 0)
                    callback?.Raise(args.BytesReceived / (double) args.TotalBytesToReceive * 100,
                        $"{ByteSizeHelper.ToString(args.BytesReceived)}/{ByteSizeHelper.ToString(args.TotalBytesToReceive)} @ {ByteSizeHelper.ToString(bytesPerSecond, mode: ByteSizeMode.Network)}/s");
            };
            sw.Start();
            try
            {
                await client.DownloadFileTaskAsync(url, file);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {
                sw.Stop();
            }
        }
    }
}