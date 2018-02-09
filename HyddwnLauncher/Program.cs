using System;
using System.IO;
using System.Reflection;
using CefSharp;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Cef.EnableHighDPISupport();
            Cef.Initialize(new CefSettings());

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