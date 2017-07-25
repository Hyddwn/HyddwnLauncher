using System.Collections.Generic;

namespace HyddwnLauncher.Data
{
    public interface IDatabase
    {
        /// <summary>
        ///     Amount of entries.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     List of exceptions caught while loading the database.
        /// </summary>
        List<DatabaseWarningException> Warnings { get; }

        /// <summary>
        ///     Removes all entries.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Loads file if it exists, raises exception otherwise.
        /// </summary>
        /// <param name="path">File to load</param>
        /// <param name="clear">Clear database before loading?</param>
        /// <returns></returns>
        int Load(string path, bool clear);

        /// <summary>
        ///     Loads multiple files, ignores missing ones.
        /// </summary>
        /// <param name="files">Files to load</param>
        /// <param name="cache">Path to an optional cache file (null for none)</param>
        /// <param name="clear">Clear database before loading?</param>
        /// <returns></returns>
        int Load(string[] files, string cache, bool clear);
    }
}