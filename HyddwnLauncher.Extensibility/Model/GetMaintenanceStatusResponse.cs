using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct GetMaintenanceStatusResponse
    {
        [JsonProperty(PropertyName = "maintenanceNo")]
        public int MaintenanceNumber { get; }

        [JsonProperty(PropertyName = "maintenanceMode")]
        public string MaintenanceMode { get; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; }
    }
}
