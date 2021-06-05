using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Models
{
    public class TopicActionObjectEnvelope
    {
        [JsonProperty("topicActionObject")]
        public TopicActionObject TopicActionObject;

        [JsonProperty("shortURL")]
        public string ShortURL;
    }
}
