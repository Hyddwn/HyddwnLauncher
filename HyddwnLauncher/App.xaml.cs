using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Windows;
using HyddwnLauncher.Core;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string Assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string Assemblypath = Path.GetDirectoryName(Assembly);
        public static string[] CmdArgs;

        protected override void OnStartup(StartupEventArgs e)
        {
            var packFileClean = false;

            if (!Directory.Exists($@"{Assemblypath}\Logs\Hyddwn Launcher"))
                Directory.CreateDirectory($@"{Assemblypath}\Logs\Hyddwn Launcher");
            Log.LogFile = $@"{Assemblypath}\Logs\Hyddwn Launcher\Hyddwn Launcher-{DateTime.Now:yyyy-MM-dd_hh-mm}.log";

            Log.Info("=== Application Startup ===");

            Log.Info("Initialize Launcher Context");
            var launcherContext = new LauncherContext();

#if DEBUG
            launcherContext.LauncherSettingsManager.Reset();
#endif

            CmdArgs = Environment.GetCommandLineArgs();
            for (var index = 0; index != e.Args.Length; ++index)
            {
                if (e.Args[index].Contains("/?") || e.Args[index].Contains("-?"))
                {
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.usage__);
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.usage___);
                    Environment.Exit(0);
                }

                if (e.Args[index].Contains("/noadmin"))
                    launcherContext.LauncherSettingsManager.LauncherSettings.RequiresAdmin = false;
                if (e.Args[index].Contains("/clean"))
                    packFileClean = true;
            }

            if (packFileClean)
            {
                WaitForProcessEndThenCleanPack();
                return;
            }

#if DEBUG
            launcherContext.LauncherSettingsManager.LauncherSettings.RequiresAdmin = false;
            launcherContext.LauncherSettingsManager.LauncherSettings.FirstRun = true;
#endif
            CheckForAdmin(launcherContext.LauncherSettingsManager.LauncherSettings.RequiresAdmin);
            ServicePointManager.DefaultConnectionLimit =
                launcherContext.LauncherSettingsManager.LauncherSettings.ConnectionLimit;
            Log.Info("Application initialized, loading main window.");
            var mainWindow = new SplashScreen(); //new MainWindow(launcherContext);
            Current.MainWindow = mainWindow;
            mainWindow.Show();
            base.OnStartup(e);
        }

        private void WaitForProcessEndThenCleanPack()
        {
            var clientCount = 0;

            var mabiClients = Process.GetProcessesByName("client");

            foreach (var mabiClient in mabiClients)
            {
                clientCount++;
                mabiClient.EnableRaisingEvents = true;
                mabiClient.Exited += (sender, args) =>
                {
                    clientCount--;
                    if (clientCount != 0) return;
                    var location = mabiClient.MainModule.FileName;
                    var locationPath = Path.GetDirectoryName(location);
                    if (locationPath == null) return;
                    var packPath = Path.Combine(locationPath, "package");
                    var files = Directory.GetFiles(packPath);
                    foreach (var file in from file in files
                        let fileName = Path.GetFileName(file)
                        where fileName.Contains("hy_")
                        select file)
                        File.Delete(file);
                    Shutdown();
                };
            }
        }

        private static void CheckForAdmin(bool requiresAdmin)
        {
            if (!requiresAdmin || IsAdministrator())
                return;
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            };
            if (Assembly != null)
                startInfo.FileName = Assembly;
            startInfo.Arguments = string.Join(" ", Environment.GetCommandLineArgs());
            startInfo.Verb = "runas";
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Log.Error("Cannot start elevated instance:\r\n{0}", (object) ex);
            }

            Environment.Exit(0);
        }

        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("=== Application Shutdown ===");
            base.OnExit(e);
        }
    }
}