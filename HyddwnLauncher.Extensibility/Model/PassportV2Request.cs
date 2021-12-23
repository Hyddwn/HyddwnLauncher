using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class PassportV2Request
    {
        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }
    }
}
