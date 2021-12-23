using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     The object representing the response received when attempting to get launcher configuration data.
    /// </summary>
    public struct GameBuildConfigurationV1Response
    {
        [JsonProperty("directory")]
        public string Directory { get; set; }

        [JsonProperty("directoryName")]
        public string DirectoryName { get; set; }

        [JsonProperty("executablePath")]
        public string ClientPath { get; set; }

        [JsonProperty("executablePathBit64")]
        public string ClientPath64 { get; set; }

        [JsonProperty("parameter")]
        public List<string> Arguments { get; set; }

        [JsonProperty("patch")]
        public bool IsPatchAvailable { get; set; }

        [JsonProperty("productId")]
        public int ProductId { get; set; }

        [JsonProperty("public")]
        public bool IsPublic { get; set; }

        [JsonProperty("requiredDiskSpace")]
        public string RequiredDiskSpace { get; set; }

        [JsonProperty("requiredInstallationSoftware")]
        public List<string> RequiredInstallationSoftware { get; set; }

        [JsonProperty("requiredUac")]
        public bool RequireUAC { get; set; }

        [JsonProperty("shortCutName")]
        public string ShortCutName { get; set; }

        [JsonProperty("supportAutomaticUpdate")]
        public bool AutoUpdateSupported { get; set; }

        [JsonProperty("supportedOs")]
        public SupportedOs SupportedOs { get; set; }

        [JsonProperty("supportedSystemType")]
        public SupportedArch SupportedArch { get; set; }

        [JsonProperty("workingDirectory")]
        public string WorkingDirectory { get; set; }
    }
}
