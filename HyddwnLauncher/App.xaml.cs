using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
        private static readonly string AssemblyFilePath = Assembly.GetExecutingAssembly().Location;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyFilePath);
        public static string[] CmdArgs;

        protected override void OnStartup(StartupEventArgs e)
        {
            var packFileClean = false;

            var baseDirectory = $@"{AssemblyDirectory}\Logs\Hyddwn Launcher\";

            if (!Directory.Exists(baseDirectory))
                Directory.CreateDirectory(baseDirectory);

            var logFile = Path.Combine(baseDirectory, $"Hyddwn Launcher-{DateTime.Now:yyyy-MM-dd_hh-mm.fff}.log");

            Log.LogFile = logFile;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var launcherVersionString = $"{version.Major}.{version.Minor}.{version.Build}";
            var betaVersion = $"{version.Revision}";

            Log.Info(HyddwnLauncher.Properties.Resources.AppStartup);
            Log.Info(HyddwnLauncher.Properties.Resources.HyddwnLauncherVersion, launcherVersionString);

            Log.Info(HyddwnLauncher.Properties.Resources.InitializingLauncherContext);
            var launcherContext = new LauncherContext(logFile, launcherVersionString, betaVersion);

#if DEBUG
            launcherContext.LauncherSettingsManager.Reset();
#endif

            Log.Info(HyddwnLauncher.Properties.Resources.CheckingForLaunchArguments);
            CmdArgs = Environment.GetCommandLineArgs();
            for (var index = 0; index != e.Args.Length; ++index)
            {
                if (e.Args[index].Contains("/?") || e.Args[index].Contains("-?"))
                {
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.OnStartupUsage1);
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.OnStartupUsage2);
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.OnStartupUsage3);
                    Console.WriteLine("/noupdate - Disable the launcher's update check in the settings.");
                    Console.WriteLine("/nopatch - Disable the launcher's patching system in the settings.");
                    Environment.Exit(0);
                }

                if (e.Args[index].Contains("/noadmin"))
                {
                    launcherContext.LauncherSettingsManager.LauncherSettings.RequiresAdmin = false;
                    Log.Info(HyddwnLauncher.Properties.Resources.NoadminWasDeclared);
                }

                if (e.Args[index].Contains("/clean"))
                {
                    packFileClean = true;
                    Log.Info(HyddwnLauncher.Properties.Resources.CleanWasDeclared);
                }

                if (e.Args[index].Contains("/noupdate"))
                {
                    launcherContext.LauncherSettingsManager.LauncherSettings.DisableLauncherUpdateCheck = true;
                    Log.Info("noupdate was declared, disabling the launcher's update check in settings.");
                }

                if (e.Args[index].Contains("/nopatch"))
                {
                    launcherContext.LauncherSettingsManager.LauncherSettings.AllowPatching = false;
                    Log.Info("nopatch was declared, disabling the launcher's patching system in settings.");
                }
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit =
                launcherContext.LauncherSettingsManager.LauncherSettings.ConnectionLimit;
            Log.Info(
                string.Format(HyddwnLauncher.Properties.Resources.AppliedMaximumDownloadLimit,
                    launcherContext.LauncherSettingsManager.LauncherSettings.ConnectionLimit));
            Log.Info(HyddwnLauncher.Properties.Resources.ApplicationInitialized);
            var mainWindow = new MainWindow(launcherContext);
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
                clientCount++;
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
            if (AssemblyFilePath != null)
                startInfo.FileName = AssemblyFilePath;
            startInfo.Arguments = string.Join(" ", Environment.GetCommandLineArgs());
            startInfo.Verb = "runas";
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Log.Error(HyddwnLauncher.Properties.Resources.CheckForAdmin_CannotStartElevatedInstance, (object) ex);
            }

            Environment.Exit(0);
        }

        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info(HyddwnLauncher.Properties.Resources.AppShutdown);
            base.OnExit(e);
        }
    }
}