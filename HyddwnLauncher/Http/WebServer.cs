using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Util;
using Swebs;

namespace HyddwnLauncher.Http
{
    public class WebServer
    {
        private Configuration WebConf { get; set; }
        private HttpServer HttpServer { get; set; }

        public event Action<string> Completed;

        public static readonly WebServer Instance = new WebServer();

        private const string HostsFilePath = "C:\\Windows\\system32\\drivers\\etc\\hosts";

        public void Run()
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
            if (string.IsNullOrWhiteSpace(nexonLine))
            {
                lines.Add("127.0.0.1 nexon.com");
                File.WriteAllLines(HostsFilePath, lines);
            }

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

            HttpServer.Start();
        }

        public void CompletedAction(string value)
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
            if (!string.IsNullOrWhiteSpace(nexonLine))
            {
                lines.Remove(nexonLine);
                File.WriteAllLines(HostsFilePath, lines);
            }

            Completed?.Raise(value);
        }

        public void Stop()
        {
            HttpServer.Stop();
            HttpServer.Dispose();
            HttpServer = null;
        }
    }
}
