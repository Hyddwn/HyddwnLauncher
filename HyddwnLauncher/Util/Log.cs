using System;
using System.IO;

namespace HyddwnLauncher.Util
{
    public static class Log
    {
        private static string _logFile;
        private static StreamWriter _file;
        private static readonly object _lockObject = new object();

        public static string Archive { get; set; }

        public static string LogFile
        {
            get => _logFile;
            set
            {
                if (value != null)
                {
                    var directoryName = Path.GetDirectoryName(value);
                    if (directoryName != null && !Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    if (File.Exists(value))
                    {
                        if (Archive != null)
                        {
                            if (!Directory.Exists(Archive))
                                Directory.CreateDirectory(Archive);
                            var str1 = Path.Combine(Archive, File.GetCreationTime(value).ToString("yyyy-MM-dd_hh-mm"));
                            var str2 = Path.Combine(str1, Path.GetFileName(value));
                            if (!Directory.Exists(str1))
                                Directory.CreateDirectory(str1);
                            if (File.Exists(str2))
                                File.Delete(str2);
                            File.Move(value, str2);
                        }
                        File.Delete(value);
                    }
                }
                _logFile = value;
            }
        }

        public static void Info(string format, params object[] args)
        {
            WriteLine(LogLevel.Info, format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            WriteLine(LogLevel.Warning, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            WriteLine(LogLevel.Error, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            WriteLine(LogLevel.Debug, format, args);
        }

        public static void Debug(object obj)
        {
            WriteLine(LogLevel.Debug, obj.ToString());
        }

        public static void Status(string format, params object[] args)
        {
            WriteLine(LogLevel.Status, format, args);
        }

        public static void Exception(Exception ex, string description = null, params object[] args)
        {
            if (description != null)
                WriteLine(LogLevel.Error, description, args);
            WriteLine(LogLevel.Exception, ex.ToString());
        }

        private static void WriteLine(LogLevel level, string format, params object[] args)
        {
            Write(level, format + Environment.NewLine, args);
        }

        private static void Write(LogLevel level, string format, params object[] args)
        {
            lock (_lockObject)
            {
                if (_logFile == null)
                    return;
                if (_file == null)
                    _file = new StreamWriter(_logFile, true);
                _file.Write(DateTime.Now + " ");
                if (level != LogLevel.None)
                    _file.Write("[{0}] - ", level);
                _file.Write(format, args);
                if (MainWindow.Instance != null)
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.AddToLog(format, args));
                _file.Flush();
            }
        }
    }
}