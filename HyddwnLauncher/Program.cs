using System;
using System.IO;
using System.Reflection;
using System.Windows;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var assemblypath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                try
                {
                    try
                    {
                        if (!Directory.Exists($@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions"))
                            Directory.CreateDirectory($@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions");

                        File.WriteAllText(
                            $@"{assemblypath}\Logs\Hyddwn Launcher\Exceptions\Unhandled_Exception-{DateTime.Now:yyyy-MM-dd_hh-mm.fff}.log",
                            string.Format(Resources.HyddwnLauncherVersion, 1) +
                            string.Format(Resources.UnhandledExceptionFileSegmentException, eventArgs.ExceptionObject));
                    }
                    catch
                    {
                        Clipboard.SetText(eventArgs.ExceptionObject.ToString());
                        MessageBox.Show(
                            string.Format(
                                Resources.UnhandledExceptionFileError,
                                eventArgs.ExceptionObject));
                        Environment.Exit(1);
                    }

                    Log.Exception((Exception) eventArgs.ExceptionObject, Resources.UnhandledExceptionFatalError);
                    new ExceptionReporter((Exception) eventArgs.ExceptionObject).ShowDialog();
                }
                catch
                {
                    Environment.Exit(1);
                }
                finally
                {
                    Environment.Exit(1);
                }
            };
            App.Main();
        }
    }
}