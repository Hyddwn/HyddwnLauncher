using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct GameAuth2AccessV1Request
    {
        /// <summary>
        ///     The product id for the game being checked
        /// </summary>
        [JsonProperty("productId")]
        public string ProductId { get; set; }
    }
}
