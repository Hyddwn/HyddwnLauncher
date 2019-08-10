using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class LauncherConfigResponse
    {
        [JsonProperty("trackingServiceId")]
        public object TrackingServiceId { get; set; }

        [JsonProperty("useNewDeployProcess")]
        public bool UserNewDeployProcess { get; set; }

        [JsonProperty("autoUpdateSupported")]
        public bool AutoUpdateSupported { get; set; }

        [JsonProperty("deactiveDiffPatch")]
        public object DeactivateDiffPatch { get; set; }

        [JsonProperty("launchConfig")]
        public LaunchConfig LaunchConfig { get; set; }

        [JsonProperty("predefinedInstallScripts")]
        public List<string> PredefinedInstallScripts { get; set; }

        [JsonProperty("requiredDiskSpace")]
        public string RequiredDiskSpace { get; set; }

        [JsonProperty("supportedOs")]
        public SupportedOs SupportedOs { get; set; }

        [JsonProperty("supportedArch")]
        public SupportedArch SupportedArch { get; set; }
    }
}
