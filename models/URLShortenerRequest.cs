using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudWithChris.Integrations.Approvals.models
{
    public class URLShortenerRequest
    {
        [JsonProperty("tagUtm")]
        public bool TagUtm = true;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("tagWt")]
        public bool TagWt = true;

        [JsonProperty("campaign")]
        public string Campaign = "link";

        [JsonProperty("mediums")]
        public List<string> Mediums;

        [JsonProperty("input")]
        public string Input;
    }

}
