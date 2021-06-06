using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Models
{
    public class ContentAndActionObject
    {
        public string code;
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

        [JsonProperty("actions")]
        public List<Action> Actions;
    }

    public class Action
    {
        [JsonProperty("actionTypes")]
        public List<string> ActionTypes;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("platforms")]
        public List<string> Platforms;
    }
}
