using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     The error response object
    /// </summary>
    public struct ErrorResponse
    {
        /// <summary>
        ///     The code for the error
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        ///     The error description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        ///     The untranslated message (the launcher usually does this)
        /// </summary>
        public string Message { get; set; }
    }
}
