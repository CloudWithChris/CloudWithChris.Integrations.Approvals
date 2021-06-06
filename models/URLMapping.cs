using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Models
{
    public class URLMapping
    {
        [JsonProperty("ShortUrl")]
        public string ShortUrl;

        [JsonProperty("LongUrl")]
        public string LongUrl;
    }

}
