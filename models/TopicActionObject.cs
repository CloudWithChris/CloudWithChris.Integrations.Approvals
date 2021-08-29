using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Models
{
    public class TopicActionObject
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("contentType")]
        public string ContentType;

        [JsonProperty("source")]
        public string Source;

        [JsonProperty("summary")]
        public string Summary;

        [JsonProperty("actionType")]
        public string ActionType;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("platform")]
        public string Platform;
        [JsonProperty("metadata")]
        public Metadata Metadata;
        [JsonProperty("subreddit")]
        public string Subreddit;
        [JsonProperty("scheduledDateTime")]
        public DateTime ScheduledDateTime;
        [JsonProperty("flair")]
        public string Flair;

    }
}
