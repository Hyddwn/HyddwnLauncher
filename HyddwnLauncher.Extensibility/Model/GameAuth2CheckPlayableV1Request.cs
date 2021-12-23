using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct GameAuth2CheckPlayableV1Request
    {
        /// <summary>
        ///     The product id for the game being checked
        /// </summary>
        [JsonProperty("productId")]
        public string ProductId { get; set; }
    }
}
