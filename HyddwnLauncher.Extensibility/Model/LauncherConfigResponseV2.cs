using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// The object representing the response received when attempting to get launcher configuration data.
    /// </summary>
    public class LauncherConfigResponseV2
    {
        [JsonProperty("required_uac")]
        public bool RequireUAC { get; set; }

        [JsonProperty("required_installation_software")]
        public List<string> PredefinedInstallScripts { get; set; }

        [JsonProperty("working_directory")]
        public string WorkingDirectory { get; set; }

        [JsonProperty("supported_os")]
        public SupportedOsV2 SupportedOs { get; set; }

        [JsonProperty("directory_name")]
        public string DirectoryName { get; set; }

        [JsonProperty("executable_path_bit64")]
        public string ClientPath64 { get; set; }

        [JsonProperty("patch")]
        public bool IsPatchAvailable { get; set; }

        [JsonProperty("support_automatic_update")]
        public bool AutoUpdateSupported { get; set; }

        [JsonProperty("public")]
        public bool IsPublic { get; set; }

        [JsonProperty("required_disk_space")]
        public string RequiredDiskSpace { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("parameter")]
        public List<string> Arguments { get; set; }

        [JsonProperty("supported_system_type")]
        public SupportedArch SupportedArch { get; set; }

        [JsonProperty("executable_path")]
        public string ClientPath { get; set; }

        [JsonProperty("tracking_id")]
        public object TrackingId { get; set; }

        [JsonProperty("short_cut_name")]
        public string ShortCutName { get; set; }
    }
}