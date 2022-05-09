using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HyddwnLauncher.Core;
using HyddwnLauncher.Util;
using HyddwnLauncher.Util.Helpers;
using Swebs;

namespace HyddwnLauncher.Http
{
    public class WebServer
    {
        private Configuration WebConf { get; set; }
        private HttpServer HttpServer { get; set; }

        public bool HostFileFailure { get; private set; }

        private event Action<string> Completed;

        public static readonly WebServer Instance = new WebServer();

        private const string HostsFilePath = "C:\\Windows\\system32\\drivers\\etc\\hosts";

        public async Task<string> RunAsync()
        {
            Log.Info("Initializing Captcha Bypass WebServer...");

            if (WebConf == null)
            {
                WebConf = new Configuration();
                WebConf.Port = 80;
                WebConf.AllowDirectoryListing = false;
                WebConf.SourcePaths.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "web"));
            }

            if (HttpServer == null)
            {
                HttpServer = new HttpServer(WebConf);
                HttpServer.RequestReceived += (sender, args) => Log.Debug("[{0}] - {1}", args.Request.HttpMethod, args.Request.Path);
                HttpServer.UnhandledException += (sender, args) => Log.Exception(args.Exception, args.Exception.Source);
            }

            if (!SetHostsFile())
                return null;

            try
            {
                Log.Info("Starting WebServer...");
                HttpServer.Start();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to start the WebServer. Possible port or permission issue.");
                try
                {
                    var lines = File.ReadAllLines(HostsFilePath).ToList();
                    var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                    if (!string.IsNullOrWhiteSpace(nexonLine))
                    {
                        lines.Remove(nexonLine);
                        File.WriteAllLines(HostsFilePath, lines);
                        HostFileFailure = false;
                    }

                    lines = File.ReadAllLines(HostsFilePath).ToList();
                    nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                    if (!string.IsNullOrWhiteSpace(nexonLine))
                    {
                        Log.Error("The sanity check for the HOSTS file failed, falling back!");
                        HostFileFailure = true;
                        return null;
                    }
                }
                catch (Exception ex2)
                {
                    // Should never be called as non admin...
                    Log.Exception(ex2, "Failed to read or edit the HOSTS file. {0}",
                        App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                    HostFileFailure = true;
                    return null;
                }

                return null;
            }

            string recaptchaToken = null;

            Completed += s => recaptchaToken = s;

            Log.Info("Starting the browser...");
            RunAsDesktopUser(BrowserHelper.GetDefaultBrowserPath(), "http://nexon.com");

            while (recaptchaToken == null)
                await Task.Delay(100);

            return recaptchaToken;
        }

        public void CompletedAction(string value)
        {
            Log.Info("Received response from the captcha bypass.");
            UnsetHostsFile();
            Completed?.Raise(value);
        }

        public bool SetHostsFile()
        {
            Log.Info("Attempting to modify the HOSTS file...");

            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (string.IsNullOrWhiteSpace(nexonLine))
                {
                    lines.Add("127.0.0.1 nexon.com");
                    File.WriteAllLines(HostsFilePath, lines);
                    Log.Info("HOSTS file set.");
                }
            }
            catch (Exception ex)
            {
                // Should never be called as non admin...
                Log.Exception(ex, "Failed to read or edit the HOSTS file. {0}", App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                return false;
            }

            Log.Info("HOSTS file sanity check...");
            // SanityCheck
            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (string.IsNullOrWhiteSpace(nexonLine))
                {
                    Log.Error("The sanity check for the HOSTS file failed, falling back!");
                    return false;
                }

                Log.Info("HOSTS file sanity check passed!");

            }
            catch (Exception ex)
            {
                Log.Exception(ex, "The sanity check for the HOSTS file failed, falling back!");
                return false;
            }

            Log.Info("Flushing DNS cache to force change to take effect...");

            Process.Start(new ProcessStartInfo
            {
                Arguments = "/flushdns",
                CreateNoWindow = true,
                FileName = "ipconfig",
                UseShellExecute = false
            });

            return true;
        }

        public void UnsetHostsFile()
        {
            Log.Info("Attempting to revert the HOSTS file...");

            try
            {
                var lines = File.ReadAllLines(HostsFilePath).ToList();
                var nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (!string.IsNullOrWhiteSpace(nexonLine))
                {
                    lines.Remove(nexonLine);
                    File.WriteAllLines(HostsFilePath, lines);
                    HostFileFailure = false;
                }

                Log.Info("HOSTS file sanity check...");

                lines = File.ReadAllLines(HostsFilePath).ToList();
                nexonLine = lines.FirstOrDefault(l => l.Contains("nexon.com"));
                if (!string.IsNullOrWhiteSpace(nexonLine))
                {
                    Log.Error("The sanity check for the HOSTS file failed, falling back!");
                    HostFileFailure = true;
                }

                Log.Info("HOSTS file unset.");
            }
            catch (Exception ex2)
            {
                // Should never be called as non admin...
                Log.Exception(ex2, "Failed to read or edit the HOSTS file. {0}",
                    App.IsAdministrator() ? "Running as admin." : "NOT running as admin.");
                HostFileFailure = true;
            }
        }

        public void Stop()
        {
            if (HttpServer.State != HttpServerState.Started)
                HttpServer.Stop();
            HttpServer.Dispose();
            HttpServer = null;
        }

        public static void RunAsDesktopUser(string fileName, string args)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

            // To start process as shell user you will need to carry out these steps:
            // 1. Enable the SeIncreaseQuotaPrivilege in your current token
            // 2. Get an HWND representing the desktop shell (GetShellWindow)
            // 3. Get the Process ID(PID) of the process associated with that window(GetWindowThreadProcessId)
            // 4. Open that process(OpenProcess)
            // 5. Get the access token from that process (OpenProcessToken)
            // 6. Make a primary token with that token(DuplicateTokenEx)
            // 7. Start the new process with that primary token(CreateProcessWithTokenW)

            var hProcessToken = IntPtr.Zero;
            // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
            try
            {
                var process = Unmanaged.GetCurrentProcess();
                if (!Unmanaged.OpenProcessToken(process, 0x0020, ref hProcessToken))
                    return;

                var tkp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES[1]
                };

                if (!Unmanaged.LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid))
                    return;

                tkp.Privileges[0].Attributes = 0x00000002;

                if (!Unmanaged.AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero))
                    return;
            }
            finally
            {
                Unmanaged.CloseHandle(hProcessToken);
            }

            // Get an HWND representing the desktop shell.
            // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
            // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
            // restarted elevated.
            var hwnd = Unmanaged.GetShellWindow();
            if (hwnd == IntPtr.Zero)
                return;

            var hShellProcess = IntPtr.Zero;
            var hShellProcessToken = IntPtr.Zero;
            var hPrimaryToken = IntPtr.Zero;
            try
            {
                // Get the PID of the desktop shell process.
                uint dwPID;
                if (Unmanaged.GetWindowThreadProcessId(hwnd, out dwPID) == 0)
                    return;

                // Open the desktop shell process in order to query it (get the token)
                hShellProcess = Unmanaged.OpenProcess(ProcessAccessFlags.QueryInformation, false, dwPID);
                if (hShellProcess == IntPtr.Zero)
                    return;

                // Get the process token of the desktop shell.
                if (!Unmanaged.OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken))
                    return;

                var dwTokenRights = 395U;

                // Duplicate the shell's process token to get a primary token.
                // Based on experimentation, this is the minimal set of rights required for CreateProcessWithTokenW (contrary to current documentation).
                if (!Unmanaged.DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out hPrimaryToken))
                    return;

                // Start the target process with the new token.
                var si = new STARTUPINFO();
                var pi = new PROCESS_INFORMATION();
                if (!Unmanaged.CreateProcessWithTokenW(hPrimaryToken, 0, fileName, "\"" + fileName + "\" \"" + args + "\"", 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi))
                    return;
            }
            finally
            {
                Unmanaged.CloseHandle(hShellProcessToken);
                Unmanaged.CloseHandle(hPrimaryToken);
                Unmanaged.CloseHandle(hShellProcess);
            }
        }
    }
}
