using System;
using System.IO;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Windows;
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
                        Clipboard.SetText(eventArgs.ExceptionObject.ToString());
                        MessageBox.Show(
                            $"A fatal error has occured that possibly affects logging. A copy of the following message was saved to your clipboard:\r\n{eventArgs.ExceptionObject}");
                        Environment.Exit(1);
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