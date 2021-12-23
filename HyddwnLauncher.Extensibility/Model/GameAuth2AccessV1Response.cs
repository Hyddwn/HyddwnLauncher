using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct GameAuth2AccessV1Response
    {
        /// <summary>
        ///     Whether the game is playable for this user
        /// </summary>
        [JsonProperty("isPlayable")]
        public bool IsPlayable { get; set; }

        /// <summary>
        ///     Whether user is developer
        /// </summary>
        [JsonProperty("isDeveloper")]
        public string IsDeveloper { get; set; }

        /// <summary>
        ///     Whether user is Ip blocked
        /// </summary>
        [JsonProperty("ipBlocked")]
        public string IsIpBlocked { get; set; }
    }
}
