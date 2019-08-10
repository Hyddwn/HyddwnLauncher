using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HyddwnLauncher.Extensibility.Model
{
    public class UserAvatar
    {
        [JsonProperty("avatar_id")]
        public int AvatarId { get; set; }

        [JsonProperty("avatar_img")]
        public int AvatarImageUrl { get; set; }

        [JsonProperty("is_custom_avatar")]
        public bool IsCustomAvatar { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }
    }
}
