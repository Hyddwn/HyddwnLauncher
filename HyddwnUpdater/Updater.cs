using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using HyddwnUpdater.Util;

namespace HyddwnUpdater
{
    public class Updater
    {
        public static readonly Updater Instance = new Updater();
        private static string[] _args;

        private Updater()
        {
            _args = ParseLine(Environment.CommandLine).ToArray();
        }

        private static void CheckForAdmin()
        {
            if (IsAdministrator()) return;

            var executingAssembly = Assembly.GetExecutingAssembly();

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = executingAssembly.Location,
                Arguments = $"\"{_args[1]}\" {_args[2]} \"{_args[3]}\"",
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Log.Error("Cannot start elevated instance:\r\n{0}", (object)ex);
            }
            Environment.Exit(0);
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
		/// Returns arguments parsed from line.
		/// </summary>
		/// <remarks>
		/// Matches words and multiple words in quotation.
		/// </remarks>
		/// <example>
		/// arg0 arg1 arg2 -- 3 args: "arg0", "arg1", and "arg2"
		/// arg0 arg1 "arg2 arg3" -- 3 args: "arg0", "arg1", and "arg2 arg3"
		/// </example>
        private static IEnumerable<string> ParseLine(string line)
        {
            var args = new List<string>();
            var quote = false;
            for (int i = 0, n = 0; i <= line.Length; ++i)
            {
                if ((i == line.Length || line[i] == ' ') && !quote)
                {
                    if (i - n > 0)
                        args.Add(line.Substring(n, i - n).Trim(' ', '"'));

                    n = i + 1;
                    continue;
                }

                if (line[i] == '"')
                    quote = !quote;
            }

            return args;
        }

        public void Run()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var logDirectoryName = Path.GetDirectoryName(executingAssembly.Location);
            Log.LogFile = logDirectoryName + ".\\Update.log";
            Log.Info("Application Start.");

            try
            {
                Log.Info("////// Arguments //////");
                Log.Info($"Update Archive: {_args[1]}");
                Log.Info($"SHA CHecksum: {_args[2]}");
                Log.Info($"Post Update Launch Target: {_args[3]}");
                Log.Info("///////////////////////");
            }
            catch
            {
                Console.WriteLine("One or more arguments are missing. Prese enter to exit.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            using (var changingOutput = new ChangingOutput("Terminating target process..."))
            {
                try
                {
                    Log.Info("Check that executing process is actually terminated...");
                    var proc = Process.GetProcessesByName(_args[3]);
                    Log.Info("One or more of the process(s) is still running. Terminating...");

                    foreach (var process in proc)
                        process.Kill();

                    Log.Info("Killed process successfully!");

                    Log.Info("Wait 2 seconds for application to close entirely...");
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    changingOutput.PrintResult(false);
                    Log.Exception(ex, "Process not running or access denied, update may fail.");
                }
            }



#if DEBUG
            Log.Debug("Skip administrator permissions check.");
#else
            Log.Info("Chech if Administrator...");
            CheckForAdmin();
#endif

            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Info("Starting update proceedure.");
            Log.Info($"Working Directory: {directoryName}");
            using (var changingOutput = new ChangingOutput("Verifying file..."))
            {
                try
                {
                    Log.Info("Calculating SHA256 Checksum for file...");

                    using (var fileStream = File.OpenRead(_args[1]))
                    {
                        var calculatedHash = BitConverter.ToString(new SHA256Managed().ComputeHash(fileStream)).Replace("-", string.Empty);
                        Log.Info("Expected hash {0}", _args[2]);
                        Log.Info("Got hash {0}", calculatedHash);

                        if (calculatedHash != _args[2])
                        {
                            changingOutput.PrintResult(false);
                            Log.Error("Verification failed! SHA256 hash mismatch. Press enter to exit.");
                            Console.WriteLine("Verification failed! SHA256 hash mismatch. Press enter to exit.");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        Log.Info("Hash is good!");
                        changingOutput.PrintResult(true);
                    }
                }
                catch (Exception ex)
                {
                    changingOutput.PrintResult(false);
                    Log.Exception(ex);
                    Console.WriteLine($"\r\nUpdate failed!\r\n{ex.Message}\r\n\r\nPress enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            using (var changingOutput = new ChangingOutput("Unpacking update file..."))
            {
                try
                {
                    var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(_args[1]);
                    Log.Info("Extracting update files to {0}", outputDirectory);

                    if (Directory.Exists(outputDirectory))
                        Directory.Delete(outputDirectory, true);
                    ZipFile.ExtractToDirectory(_args[1], outputDirectory);
                    Log.Info("Extracting update files complete.");
                    changingOutput.PrintResult(true);
                }
                catch (Exception ex)
                {
                    changingOutput.PrintResult(false);
                    Log.Exception(ex);
                    Console.WriteLine($"\r\nUpdate failed!\r\n{ex.Message}\r\n\r\nPress enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            using (var changingOutput = new ChangingOutput("Copying files..."))
            {
                try
                {
                    var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(_args[1]);

                    Log.Info("////// Begin File Copy //////");

                    DirectoryCopy(outputDirectory, directoryName);

                    Log.Info("/////// End File Copy ///////");
                    changingOutput.PrintResult(true);
                }
                catch (Exception ex)
                {
                    changingOutput.PrintResult(false);
                    Log.Exception(ex);
                    Console.WriteLine($"\r\nUpdate failed!\r\n{ex.Message}\r\n\r\nPress enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }

            using (var changingOutput = new ChangingOutput("Cleaning up..."))
            {
                Log.Info("Cleaning up...");

                var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(_args[1]);

                try
                {
                    Log.Info("Deleting {0}", outputDirectory);

                    Directory.Delete(outputDirectory, true);
#if DEBUG
                    
#else
                    File.Delete(_args[1]);
#endif
                    Log.Info("Deleted successfully!");
                    changingOutput.PrintResult(true);
                }
                catch (Exception ex)
                {
                    changingOutput.PrintResult(false);
                    Log.Exception(ex);
                    Console.WriteLine($"\r\nUpdate failed!\r\n{ex.Message} \r\n\r\nPress enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            try
            {
                Log.Info("Starting Application...");
                Console.WriteLine("Starting Application...");

                new Process {StartInfo = {FileName = directoryName + "\\" + _args[3]}}.Start();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Console.WriteLine("\r\nUpdate was successful but failed to automatically start application.");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {

                var temppath = Path.Combine(destDirName, file.Name);
                Log.Info("Copying {0} to {1}", file.Name, temppath);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs) return;
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}