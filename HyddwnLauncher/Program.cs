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
            var assemblypath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);   

            if (!Directory.Exists(assemblypath + "\\Archived"))
                Directory.CreateDirectory(assemblypath + "\\Archived");
            Log.Archive = assemblypath + "\\Archived";

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                try
                {
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