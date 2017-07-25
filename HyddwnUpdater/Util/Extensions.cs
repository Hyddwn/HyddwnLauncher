namespace HyddwnUpdater.Util
{
    public static class Extensions
    {
        public static string Center(this string stringToCenter, int totalLength)
        {
            return stringToCenter
                .PadLeft((totalLength - stringToCenter.Length) / 2 + stringToCenter.Length)
                .PadRight(totalLength);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}