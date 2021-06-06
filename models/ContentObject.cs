using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Models
{
    public class ContentObject : TableEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("title")]
        public string Title;


        [JsonProperty("createdTime")]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        [JsonProperty("contentType")]
        public string ContentType;

        [JsonProperty("source")]
        public string Source;

        [JsonProperty("summary")]
        public string Summary;
        [JsonProperty("actions")]
        public List<Action> Actions;
    }

    public static class ContentObjectExtensions
    {
        public static ContentObjectTable ToTable(this ContentObject todo)
        {
            return new ContentObjectTable
            {
                PartitionKey = "CONTENT",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                Title = todo.Title
            };
        }

        public static ContentObject ToContentObject(this ContentObjectTable todoTable)
        {
            return new ContentObject
            {
                Id = todoTable.RowKey,
                CreatedTime = todoTable.CreatedTime,
                Title = todoTable.Title,
                Source = todoTable.Source,
                ContentType = todoTable.ContentType,
                Actions = new List<Action>()
            };
        }
    }
    public class ContentObjectTable : TableEntity
    {
        public DateTime CreatedTime { get; set; }

        public string Title { get; set; }
        public string Source { get; set; }
        public string ContentType { get; set; }
        public string Summary { get; set; }
    }

}

