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
        private static string[] args;

        public Updater()
        {
            args = ParseLine(Environment.CommandLine).ToArray();
        }

        private static void CheckForAdmin()
        {
            if (IsAdministrator()) return;

            var executingAssembly = Assembly.GetExecutingAssembly();
            var logDirectoryName = Path.GetDirectoryName(executingAssembly.Location);

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            };
            if (executingAssembly.Location != null)
                startInfo.FileName = executingAssembly.Location;
            startInfo.Arguments = $"\"{args[1]}\" {args[2]} \"{args[3]}\"";
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

        /// <summary>
        ///     Returns arguments parsed from line.
        /// </summary>
        /// <remarks>
        ///     Matches words and multiple words in quotation.
        /// </remarks>
        /// <example>
        ///     arg0 arg1 arg2 -- 3 args: "arg0", "arg1", and "arg2"
        ///     arg0 arg1 "arg2 arg3" -- 3 args: "arg0", "arg1", and "arg2 arg3"
        /// </example>
        protected IList<string> ParseLine(string line)
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
                Log.Info($"Update Archive: {args[1]}");
                Log.Info($"SHA CHecksum: {args[2]}");
                Log.Info($"Post Update Launch Target: {args[3]}");
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
                    var proc = Process.GetProcessesByName(args[3]);
                    if (proc != null)
                    {
                        Log.Info("One or more of the process(s) is still running. Terminating...");

                        foreach (var process in proc)
                            process.Kill();

                        Log.Info("Killed process successfully!");

                        Log.Info("Wait 2 seconds for application to close entirely...");
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, "Failed to close one or more processes.");
                }
            }


            try
            {
            }
            catch
            {
                Log.Warning("Process not running or access denied, update may fail.");
            }

            Log.Info("Check if Administrator...");
            CheckForAdmin();


            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Info("Starting update proceedure.");
            Log.Info($"Working Directory: {directoryName}");
            using (var changingOutput = new ChangingOutput("Verifying file..."))
            {
                try
                {
                    Log.Info("Calculating SHA256 Checksum for file...");
                    using (var fileStream = File.OpenRead(args[1]))
                    {
                        var calculatedHash = BitConverter.ToString(new SHA256Managed().ComputeHash(fileStream))
                            .Replace("-", string.Empty);
                        Log.Info("Expected hash {0}", args[2]);
                        Log.Info("Got hash {0}", calculatedHash);

                        if (calculatedHash != args[2])
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
                    var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(args[1]);
                    Log.Info("Extracting update files to {0}", outputDirectory);

                    if (Directory.Exists(outputDirectory))
                        Directory.Delete(outputDirectory, true);
                    ZipFile.ExtractToDirectory(args[1], outputDirectory);
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
                    var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(args[1]);

                    Log.Info("////// Begin File Copy //////");

                    // Now Create all of the directories
                    foreach (string dirPath in Directory.GetDirectories(outputDirectory, "*",
                        SearchOption.AllDirectories))
                    {
                        if (!Directory.Exists(dirPath.Replace(outputDirectory, directoryName)))
                            Directory.CreateDirectory(dirPath.Replace(outputDirectory, directoryName));
                    }

                    // Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(outputDirectory, "*.*",
                        SearchOption.AllDirectories))
                    {
                        if (newPath.Replace(outputDirectory, directoryName) == directoryName + @"\Updater.exe")
                            File.Move(directoryName + @"\Updater.exe", directoryName + @"\Updater.old");
                        
                        Log.Info("Copying {0} to {1}", newPath, newPath.Replace(outputDirectory, directoryName));
                        File.Copy(newPath, newPath.Replace(outputDirectory, directoryName), true);
                    }
                    
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

                var outputDirectory = directoryName + "\\" + Path.GetFileNameWithoutExtension(args[1]);

                try
                {
                    Log.Info("Deleting {0}", outputDirectory);

                    Directory.Delete(outputDirectory, true);
                    File.Delete(args[1]);
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


                new Process
                {
                    StartInfo =
                    {
                        FileName = directoryName + "\\" + args[3]
                    }
                }.Start();
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
    }
}