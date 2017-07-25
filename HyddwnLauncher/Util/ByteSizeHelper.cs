namespace HyddwnLauncher.Util
{
    public static class ByteSizeHelper
    {
        private static readonly string[] BinaryStoragePrefixes =
        {
            "B",
            "KiB",
            "MiB",
            "GiB",
            "TiB"
        };

        private static readonly string[] DecimalStoragePrefixes =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        private static readonly string[] BinarySpeedPrefixes =
        {
            "b",
            "Kib",
            "Mib",
            "Gib",
            "Tib"
        };

        private static readonly string[] DecimalSpeedPrefixes =
        {
            "b",
            "Kb",
            "Mb",
            "Gb",
            "Tb"
        };

        public static string ToString(double bytes, ByteSizeSystem system = ByteSizeSystem.Decimal,
            ByteSizeMode mode = ByteSizeMode.Storage)
        {
            var returnStatement = "";
            var divisor = system == ByteSizeSystem.Decimal ? 1000 : 1024;
            var working = bytes;

            var index = 0;
            while (working >= divisor)
            {
                working /= divisor;
                ++index;
            }

            if (system == ByteSizeSystem.Decimal)
                switch (mode)
                {
                    default:
                    {
                        returnStatement = $"{working:N2} {DecimalStoragePrefixes[index]}";
                        break;
                    }
                    case ByteSizeMode.Network:
                    {
                        returnStatement = $"{working:N2} {DecimalSpeedPrefixes[index]}";
                        break;
                    }
                }
            else
                switch (mode)
                {
                    default:
                    {
                        returnStatement = $"{working:N2} {BinaryStoragePrefixes[index]}";
                        break;
                    }
                    case ByteSizeMode.Network:
                    {
                        returnStatement = $"{working:N2} {BinarySpeedPrefixes[index]}";
                        break;
                    }
                }
            return returnStatement;
        }
    }

    public enum ByteSizeSystem
    {
        Binary,
        Decimal
    }

    public enum ByteSizeMode
    {
        Storage,
        Network
    }
}