using System;

namespace HyddwnUpdater.Util
{
    [Flags]
    public enum LogLevel
    {
        Info = 0x0001,
        Warning = 0x0002,
        Error = 0x0004,
        Debug = 0x0008,
        Status = 0x0010,
        Exception = 0x0020,
        None = 0x7FFF
    }
}