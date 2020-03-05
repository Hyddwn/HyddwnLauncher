using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility.Model;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;
using Sentry;
using Sentry.Protocol;

namespace HyddwnLauncher
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");

            var sentryOptions = new SentryOptions
            {
                BeforeSend = BeforeSend,
                Dsn = new Dsn("https://62a499d239a54e118ea5770379c06b37@sentry.io/1795758"),
            };

            using (SentrySdk.Init(sentryOptions))
            {
                App.Main();
            }
        }

        private static SentryEvent BeforeSend(SentryEvent arg)
        {
            var assemblypath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var send = true;
            arg.SetTag("admin", App.IsAdministrator().ToString());
            arg.SetTag("beta", (!string.IsNullOrWhiteSpace(LauncherContext.Instance.BetaVersion)).ToString());

            try
            {
                try
                {
                    if (!Directory.Exists($@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions"))
                        Directory.CreateDirectory($@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions");

                    File.WriteAllText(
                        $@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions\Unhandled_Exception-{DateTime.Now:yyyy-MM-dd_hh-mm.fff}.log",
                        string.Format(Resources.HyddwnLauncherVersion, 1) +
                        string.Format(Resources.UnhandledExceptionFileSegmentException, arg.Exception));
                }
                catch
                {
                    Clipboard.SetText(arg.Exception.ToString());
                    MessageBox.Show(
                        string.Format(
                            Resources.UnhandledExceptionFileError,
                            arg.Exception));
                    return arg;
                }

                Log.Exception(arg.Exception, Resources.UnhandledExceptionFatalError);
                var exceptionReporter = new ExceptionReporter(arg.Exception);
                exceptionReporter.ShowDialog();
                send = exceptionReporter.Result;
            }
            catch
            {
                return arg;
            }

            return send ? arg : null;
        }
    }
}