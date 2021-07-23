using System;
using System.Collections.Generic;
using System.Text;

namespace CloudWithChris.Integrations.Approvals.models
{
    public class RedditAuthResult
    {
        public string result { get; set; }
        public int id { get; set; }
        public object exception { get; set; }
        public int status { get; set; }
        public bool isCanceled { get; set; }
        public bool isCompleted { get; set; }
        public bool isCompletedSuccessfully { get; set; }
        public int creationOptions { get; set; }
        public object asyncState { get; set; }
        public bool isFaulted { get; set; }
    }

    public class TokenResult
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }


    public class Flair
    {
        public string type { get; set; }
        public bool text_editable { get; set; }
        public string allowable_content { get; set; }
        public string text { get; set; }
        public int max_emojis { get; set; }
        public string text_color { get; set; }
        public bool mod_only { get; set; }
        public string css_class { get; set; }
        public List<object> richtext { get; set; }
        public string background_color { get; set; }
        public string id { get; set; }
    }
}
