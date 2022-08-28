using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyddwnLauncher.Core;
using HyddwnLauncher.Util;
using HyddwnLauncher.Util.Helpers;
using Swebs;

namespace HyddwnLauncher.Http
{
    public class WebServer
    {
        private Configuration WebConf { get; set; }
        private HttpServer HttpServer { get; set; }

        public bool HostFileFailure { get; private set; }

        private event Action<string> Completed;

        public static readonly WebServer Instance = new WebServer();

        private const string HostsFilePath = "C:\\Windows\\system32\\drivers\\etc\\hosts";

        public async Task<string> RunAsync()
        {
            Log.Info("Initializing Captcha Bypass WebServer...");

            if (WebConf == null)
            {
                WebConf = new Configuration();
                WebConf.Port = 80;
                WebConf.AllowDirectoryListing = false;
                WebConf.SourcePaths.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "web"));
            }

            if (HttpServer == null)
            {
                HttpServer = new HttpServer(WebConf);
                HttpServer.RequestReceived += (sender, args) => Log.Debug("[{0}] - {1}", args.Request.HttpMethod, args.Request.Path);
                HttpServer.UnhandledException += (sender, args) => Log.Exception(args.Exception, args.Exception.Source);
            }

            if (!SetHostsFile())
                return null;

            try
            {
                Log.Info("Starting WebServer...");
                HttpServer.Start();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to start the WebServer. Possible port or permission issue.");
                try
                {
                    var lines = File.ReadAllLines(HostsFilePath).ToList();
                    var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                    if (!string.IsNullOrWhiteSpace(nexonLine))
                    {
                        lines.Remove(nexonLine);
                        File.WriteAllLines(HostsFilePath, lines);
                        HostFileFailure = false;
                    }

                    lines = File.ReadAllLines(HostsFilePath).ToList();
                    nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                    if (!string.IsNullOrWhiteSpace(nexonLine))
                    {
                        Log.Error("The sanity check for the HOSTS file failed, falling back!");
                        HostFileFailure = true;
                        return null;
                    }
                }
                catch (Exception ex2)
                {
                    // Should never be called as non admin...
                    Log.Exception(ex2, "Failed to read or edit the HOSTS file. {0}",
                        App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                    HostFileFailure = true;
                    return null;
                }

                return null;
            }

            string recaptchaToken = null;

            Completed += s => recaptchaToken = s;

            Log.Info("Starting the browser...");
            Unmanaged.RunAsDesktopUser(BrowserHelper.GetDefaultBrowserPath(), "http://nexon.com");

            while (recaptchaToken == null)
                await Task.Delay(100);

            return recaptchaToken;
        }

        public void CompletedAction(string value)
        {
            Log.Info("Received response from the captcha bypass.");
            UnsetHostsFile();
            Completed?.Raise(value);
        }

        public bool SetHostsFile()
        {
            Log.Info("Attempting to modify the HOSTS file...");

            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (string.IsNullOrWhiteSpace(nexonLine))
                {
                    lines.Add("127.0.0.1 nexon.com");
                    File.WriteAllLines(HostsFilePath, lines);
                    Log.Info("HOSTS file set.");
                }
            }
            catch (Exception ex)
            {
                // Should never be called as non admin...
                Log.Exception(ex, "Failed to read or edit the HOSTS file. {0}", App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                return false;
            }

            Log.Info("HOSTS file sanity check...");
            // SanityCheck
            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (string.IsNullOrWhiteSpace(nexonLine))
                {
                    Log.Error("The sanity check for the HOSTS file failed, falling back!");
                    return false;
                }

                Log.Info("HOSTS file sanity check passed!");

            }
            catch (Exception ex)
            {
                Log.Exception(ex, "The sanity check for the HOSTS file failed, falling back!");
                return false;
            }

            Log.Info("Flushing DNS cache to force change to take effect...");

            Process.Start(new ProcessStartInfo
            {
                Arguments = "/flushdns",
                CreateNoWindow = true,
                FileName = "ipconfig",
                UseShellExecute = false
            });

            return true;
        }

        public void UnsetHostsFile()
        {
            Log.Info("Attempting to revert the HOSTS file...");

            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (!string.IsNullOrWhiteSpace(nexonLine))
                {
                    lines.Remove(nexonLine);
                    File.WriteAllLines(HostsFilePath, lines);
                    HostFileFailure = false;
                }

                Log.Info("HOSTS file sanity check...");

                lines = File.ReadAllLines(HostsFilePath).ToList();
                nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (!string.IsNullOrWhiteSpace(nexonLine))
                {
                    Log.Error("The sanity check for the HOSTS file failed, falling back!");
                    HostFileFailure = true;
                }

                Log.Info("HOSTS file unset.");
            }
            catch (Exception ex2)
            {
                // Should never be called as non admin...
                Log.Exception(ex2, "Failed to read or edit the HOSTS file. {0}",
                    App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                HostFileFailure = true;
            }
        }

        public void Stop()
        {
            if (HttpServer.State != HttpServerState.Started)
                HttpServer.Stop();
            HttpServer.Dispose();
            HttpServer = null;
        }
    }
}
