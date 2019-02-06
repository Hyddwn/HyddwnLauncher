using System;
using System.IO;
using System.Reflection;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var assemblypath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
                            $"Hyddwn Launcher Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n" +
                            $"Exception {eventArgs.ExceptionObject}");
                    }
                    catch
                    {
                        Log.Info("Failed to log exception to file.");
                    }

                    Log.Exception((Exception) eventArgs.ExceptionObject, "Fatal error occured in application!");
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