using System;

namespace HyddwnLauncher.Util
{
    [Flags]
    public enum LogLevel
    {
        Info = 1,
        Warning = 2,
        Error = 4,
        Debug = 8,
        Status = 16,
        Exception = 32,
        None = 32767
    }
}