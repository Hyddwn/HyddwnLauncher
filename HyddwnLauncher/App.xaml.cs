﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using HyddwnLauncher.Core;
using HyddwnLauncher.Util;
using Sentry;

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

        public App()
        {
            DispatcherUnhandledException += (sender, args) =>
            {
                SentrySdk.CaptureException(args.Exception);
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var packFileClean = false;

            var baseDirectory = $@"{AssemblyDirectory}\Logs\Hyddwn Launcher\";

            if (!Directory.Exists(baseDirectory))
                Directory.CreateDirectory(baseDirectory);

            var logFile = Path.Combine(baseDirectory, $"Hyddwn Launcher-{DateTime.Now:yyyy-MM-dd_hh-mm.fff}.log");

            Log.LogFile = logFile;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var launcherVersionString = $"{version.Major}.{version.Minor}.{version.Build}{(version.Revision > 0 ? $" Beta {version.Revision}" : "")}";
            var betaVersion = $"{(version.Revision == 0 ? "" : version.Revision.ToString())}";

            Log.Info(HyddwnLauncher.Properties.Resources.AppStartup);
            Log.Info(HyddwnLauncher.Properties.Resources.HyddwnLauncherVersion, launcherVersionString);

            Log.Info(HyddwnLauncher.Properties.Resources.InitializingLauncherContext);
            LauncherContext.Instance.Initialize(logFile, launcherVersionString, betaVersion);

#if DEBUG
            LauncherContext.Instance.LauncherSettingsManager.Reset();
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
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.OnStartupUsage4);
                    Console.WriteLine(HyddwnLauncher.Properties.Resources.OnStartupUsage5);
                    Environment.Exit(0);
                }

                if (e.Args[index].Contains("/noadmin"))
                {
                    LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.RequiresAdmin = false;
                    Log.Info(HyddwnLauncher.Properties.Resources.NoadminWasDeclared);
                }

                if (e.Args[index].Contains("/clean"))
                {
                    packFileClean = true;
                    Log.Info(HyddwnLauncher.Properties.Resources.CleanWasDeclared);
                }

                if (e.Args[index].Contains("/noupdate"))
                {
                    LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.DisableLauncherUpdateCheck = true;
                    Log.Info(HyddwnLauncher.Properties.Resources.NoupdateWasDeclared);
                }

                if (e.Args[index].Contains("/nopatch"))
                {
                    LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.AllowPatching = false;
                    Log.Info(HyddwnLauncher.Properties.Resources.NopatchWasDeclared);
                }
            }

            if (packFileClean)
            {
                WaitForProcessEndThenCleanPack();
                return;
            }

#if DEBUG
            LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.RequiresAdmin = false;
            LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.FirstRun = true;
#endif
            CheckForAdmin(LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.RequiresAdmin);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit =
                LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.ConnectionLimit;
            Log.Info(
                string.Format(HyddwnLauncher.Properties.Resources.AppliedMaximumDownloadLimit,
                    LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.ConnectionLimit));
            Log.Info(HyddwnLauncher.Properties.Resources.ApplicationInitialized);
            var mainWindow = new MainWindow(LauncherContext.Instance);
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

            SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).Wait();

            Environment.Exit(0);
        }
    }
}