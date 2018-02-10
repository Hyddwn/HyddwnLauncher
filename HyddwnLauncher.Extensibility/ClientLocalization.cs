using System;
using System.Reflection;

namespace HyddwnLauncher.Extensibility
{
    /// <summary>
    ///     Represent the localization of the Mabinogi Client
    /// </summary>
    public static class ClientLocalization
    {
        /// <summary>
        ///     Represents the Japanese version of the game
        /// </summary>
        public const string Japan = "Japan";

        /// <summary>
        ///     Represents the Japanese Hangame version of the game
        /// </summary>
        public const string JapanHangame = "Japan Hangame";

        /// <summary>
        ///     Represents the Korean version of the game
        /// </summary>
        public const string Korea = "Korea";

        /// <summary>
        ///     Represents the Korean Test version of the game
        /// </summary>
        public const string KoreaTest = "Korea Test";

        /// <summary>
        ///     Represents the North American version of the game
        /// </summary>
        public const string NorthAmerica = "North America";

        /// <summary>
        ///     Represents the Taiwanese version of the game
        /// </summary>
        public const string Taiwan = "Taiwan";

        /// <summary>
        ///     Attempts to retrieve the ClientLocalization field name for the given value
        /// </summary>
        /// <param name="predicate">The string to test for.</param>
        /// <returns>The field representing the value or '?' if not found.</returns>
        public static string GetLocalization(string predicate)
        {
            foreach (var field in typeof(ClientLocalization).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (string.Equals((string) field.GetValue(null), predicate, StringComparison.CurrentCultureIgnoreCase))
                    return field.Name;

            return "?";
        }
    }
}