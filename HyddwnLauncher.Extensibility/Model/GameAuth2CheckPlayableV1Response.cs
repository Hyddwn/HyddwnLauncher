using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct GameAuth2CheckPlayableV1Response
    {
        /// <summary>
        ///     The playing country for the player
        /// </summary>
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }
    }
}
