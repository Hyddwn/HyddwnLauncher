using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility
{
    /// <summary>
    ///     The default patcher types
    /// </summary>
    public static class DefaultPatcherTypes
    {
        /// <summary>
        ///     The default Patcher used for most custom launches
        /// </summary>
        public const string Default = "Default";

        /// <summary>
        ///     The legacy patcher from NA still used by JP
        /// </summary>
        public const string Legacy = "Legacy";

        /// <summary>
        ///     The new patcher used only by NA
        /// </summary>
        public const string NxLauncher = "NxLauncher";
    }
}
