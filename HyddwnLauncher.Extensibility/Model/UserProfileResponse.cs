using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class UserProfileResponse
    {
        [JsonProperty("avatar")]
        public UserAvatar Avatar { get; set; }

        [JsonProperty("membership_no")]
        public int MembershipNumber { get; set; }

        [JsonProperty("presence")]
        public string Presence { get; set; }

        [JsonProperty("privacy_level")]
        public int PrivacyLevel { get; set; }

        [JsonProperty("profile_name")]
        public string ProfileName { get; set; }

        [JsonProperty("profile_name_history")]
        public List<string> ProfileNameHistory { get; set; }

        [JsonProperty("relation")]
        public string Relation { get; set; }

        [JsonProperty("relation_code")]
        public int RelationCode { get; set; }

        [JsonProperty("tagline")]
        public string TagLine { get; set; }

        [JsonProperty("user_no")]
        public int UserNumber { get; set; }
    }
}
