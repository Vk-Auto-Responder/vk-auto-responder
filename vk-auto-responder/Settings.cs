using System.Collections.Generic;
using Newtonsoft.Json;

namespace VkAutoResponder
{
    public class Settings
    {
        [JsonProperty("auth-params")]
        public AuthParams AuthParams { get; set; }

        [JsonProperty("chat-ids")]
        public IList<long> ChatIds { get; set; }

        [JsonProperty("keywords")]
        public IList<string> Keywords { get; set; }

        [JsonProperty("banned-to-all-keywords")]
        public IList<string> BannedToAllKeywords { get; set; }

        [JsonProperty("reply")]
        public string Reply { get; set; }

        public override string ToString()
        {
            return $"{{\n" +
                   $" AuthParams: {AuthParams}\n" +
                   $" ChatIds: [{string.Join(", ", ChatIds)}]\n" +
                   $" Keywords: [{string.Join(", ", Keywords)}]\n" +
                   $" BannedToAllKeywords: [{string.Join(", ", BannedToAllKeywords)}]\n" +
                   $" Reply: \"{Reply}\"\n" +
                   $"}}";
        }
    }
}