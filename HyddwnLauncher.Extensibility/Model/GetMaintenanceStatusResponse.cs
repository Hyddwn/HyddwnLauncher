using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     Represents a response from the maintenance endpoint
    /// </summary>
    public readonly struct GetMaintenanceStatusResponse
    {
        /// <summary>
        ///     The identification number for this maintenance
        /// </summary>
        [JsonProperty("maintenanceNo")]
        public int MaintenanceNumber { get; }

        /// <summary>
        ///     The type of maintenance for the requested game
        /// </summary>
        [JsonProperty("maintenanceMode")]
        public string MaintenanceMode { get; }

        /// <summary>
        ///     The message to display to the user
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }
    }
}
