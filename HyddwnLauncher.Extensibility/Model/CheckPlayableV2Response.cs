using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     The object that represents the response from playability check
    /// </summary>
    public struct CheckPlayableV2Response
    {
        /// <summary>
        ///     The id of the game that was checked
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; }
    }
}
