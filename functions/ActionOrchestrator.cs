using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using CloudWithChris.Integrations.Approvals.Models;
using System.Collections.Generic;
using CloudWithChris.Integrations.Approvals.models;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace CloudWithChris.Integrations.Approvals.Functions
{
  public static class ActionOrchestrator
    {
        [FunctionName("ActionOrchestrator")]
        public static async Task ActionOrchestratorMethod
        (
            [OrchestrationTrigger] IDurableOrchestrationContext context
        )
        {
            ContentAndActionObject contentAndActions  = context.GetInput<ContentAndActionObject>();

            // Firstly, use the RetrieveShortUrl Activity function to obtain all of the needed URL Mappings
            List<Task> taskList = new List<Task>();
            Task<List<URLMapping>> urlMappings = context.CallActivityAsync<List<URLMapping>>("RetrieveShortURL", contentAndActions);
            (await urlMappings).Where(e => e.ShortUrl.Contains("localhost")).Select(e => { e.ShortUrl = e.ShortUrl.Replace("http://localhost", "https://cloudchris.ws").Replace("cloudchris-ws-url-func.azurewebsites.net", "cloudchris.ws"); return e; }).ToList();
            
            // Loop through each action that was added
            for (int actionPtr = 0; actionPtr < contentAndActions.Actions.Count; actionPtr++)
            {
                // Within each action, loop through the ActionTypes that were selected
                for (int actionTypesPtr = 0; actionTypesPtr < contentAndActions.Actions[actionPtr].ActionTypes.Count; actionTypesPtr++)
                {
                    // And to get the matrix of those, loop through the platforms too
                    for (int platformsPtr = 0; platformsPtr < contentAndActions.Actions[actionPtr].Platforms.Count; platformsPtr++)
                    {
                        // Submit a newly shaped JSON Object to a SubOrchestrator.
                        // The Sub Orchestrator will perform a number of steps -
                        // 1. If there is a match with a medium, then go ahead
                        //    and replace the source appropriately.
                        // 2. Take the contents of above item, merge with the JSON
                        //    object and submit to the Topic.
                        string _actionType = contentAndActions.Actions[actionPtr].ActionTypes[actionTypesPtr];
                        string _platform = contentAndActions.Actions[actionPtr].Platforms[platformsPtr];
                        string _source = contentAndActions.Source;


                        if (contentAndActions.Actions[actionPtr].Metadata.ShorternUrl)
                        {
                            URLMapping shortUrlMapping = (await urlMappings).Where(e => e.LongUrl.Contains($"utm_medium={_platform}")).FirstOrDefault();

                            if (shortUrlMapping != null)
                            {
                                _source = shortUrlMapping.ShortUrl;
                            }
                        }

                        if (_platform != "reddit")
                        {
                            TopicActionObject topicActionObject = new TopicActionObject()
                            {
                                Id = contentAndActions.Id,
                                Title = contentAndActions.Title,
                                ContentType = contentAndActions.ContentType,
                                Source = _source,
                                Summary = contentAndActions.Summary,
                                ActionType = _actionType,
                                Platform = _platform,
                                Message = contentAndActions.Actions[actionPtr].Message,
                                Metadata = contentAndActions.Actions[actionPtr].Metadata
                            };

                            taskList.Add(context.CallSubOrchestratorAsync("TopicActionTransformAndSend", topicActionObject));
                        } else
                        {
                            for (int i = 0; i < contentAndActions.Actions[actionPtr].Metadata.SubReddits.Count; i++)
                            {
                                string _flair;
                                if (contentAndActions.Actions[actionPtr].Metadata.Flairs.ContainsKey(contentAndActions.Actions[actionPtr].Metadata.SubReddits[i]))
                                {
                                    _flair = contentAndActions.Actions[actionPtr].Metadata.Flairs[contentAndActions.Actions[actionPtr].Metadata.SubReddits[i]];
                                } else
                                {
                                    _flair = "";
                                }


                                // Get the object that was passed in to the method
                                TopicActionObject topicActionObject = new TopicActionObject()
                                {
                                    Title = contentAndActions.Title,
                                    ActionType = _actionType,
                                    ContentType = contentAndActions.ContentType,
                                    Message = contentAndActions.Actions[actionPtr].Message,
                                    Id = contentAndActions.Id,
                                    Platform = _platform,
                                    Source = _source,
                                    Summary = contentAndActions.Summary,
                                    Subreddit = contentAndActions.Actions[actionPtr].Metadata.SubReddits[i],
                                    Flair = _flair
                                };


                                if (topicActionObject.ActionType == "schedule")
                                {
                                    topicActionObject.ScheduledDateTime = DateTime.ParseExact($"{topicActionObject.Metadata.Schedule.Date} {topicActionObject.Metadata.Schedule.Time}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                }

                                taskList.Add(context.CallActivityAsync("SendToTopic", topicActionObject));
                            }
                        }                       
                    }
                }
            }

            // Complete the Orchestrator Function when 
            // all tasks have completed.
            await Task.WhenAll(taskList);
        }


        [FunctionName("TopicActionTransformAndSend")]
        public static async Task TopicActionTransformAndSendMethod
        (
            [OrchestrationTrigger] IDurableOrchestrationContext context
        )
        {
            // Get the object that was passed in to the method
            TopicActionObject topicActionObject = context.GetInput<TopicActionObject>();

            // By this point source url will be a short URL if Source URL replace was selected and if the medium and platforms matched up.
            // If the message contains a placeholder {{url}}, then replace that text with the source url. Do not include the url at the end otherwise.

            if (topicActionObject.Message != null)
            {
                topicActionObject.Message = topicActionObject.Message.Replace("{{url}}", topicActionObject.Source);
            } else
            {
                topicActionObject.Message = topicActionObject.Source;
            }

            if (topicActionObject.ActionType == "schedule")
            {
                topicActionObject.ScheduledDateTime = DateTime.ParseExact($"{topicActionObject.Metadata.Schedule.Date} {topicActionObject.Metadata.Schedule.Time}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }


            Task task;

            try
            {
                // Send the resulting object to the Service Bus Topic
                task = context.CallActivityAsync("SendToTopic", topicActionObject);
            await Task.WhenAll(task);


            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [FunctionName("RetrieveShortURL")]
        public async static Task<List<URLMapping>> RetrieveShortURL
        (
            [ActivityTrigger] ContentAndActionObject contentAndActionObject,
            ILogger log
        )
        {
            // Get a distinct list of the platforms request for this URL (of those that have requested to ShorternURL)
            List<List<string>> platformsAcrossActions = contentAndActionObject.Actions.Where(e => e.Metadata.ShorternUrl == true).Select(e => e.Platforms).ToList();
            List<string> flattenedList = (from list in platformsAcrossActions
                                       from item in list
                                       select item).ToList();
            List<string> platforms = flattenedList.Distinct().ToList();

            // Create the Request Object to be sent to the URL Shortener
            URLShortenerRequest urlShortenerRequest = new URLShortenerRequest()
            {
                Title = contentAndActionObject.Title,
                Input = contentAndActionObject.Source,
                Mediums = platforms
            };


            List<URLMapping> listOfURLs = new List<URLMapping>();

            if (platforms.Count > 0)
            {

                string responseData;
                Uri baseAddress = new Uri(System.Environment.GetEnvironmentVariable("UrlShortenerAPIHostname"));
                //var baseAddress = new Uri("http://localhost:7071/");
                string apiEndpoint = $"api/ShortenUrl?code={System.Environment.GetEnvironmentVariable("UrlShortenerApiKey")}";

                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    using (var response = await httpClient.PostAsync(apiEndpoint, new StringContent(JsonConvert.SerializeObject(urlShortenerRequest))))
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                }

                listOfURLs = JsonConvert.DeserializeObject<List<URLMapping>>(responseData);
            }

            return listOfURLs;
        }

        
        [FunctionName("SendToTopic")]
        public static Task SendToTopic
        (
            [ActivityTrigger] TopicActionObject topicActionObject,
            [ServiceBus("actions", Connection = "ServiceBusActionsTopicSendConnection", EntityType = EntityType.Topic)] out Message queueMessage,
            ILogger log
        )
        {
            queueMessage = new Message();
            queueMessage.Body = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(topicActionObject));
            queueMessage.UserProperties.Add("platform", topicActionObject.Platform);

            if (topicActionObject.ActionType == "schedule")
            {
                //TODO: UI currently disables schedule, because there seems to be an issue with the implementation. Work in progress.
                queueMessage.ScheduledEnqueueTimeUtc = topicActionObject.ScheduledDateTime;
                //Override the actionType to be immediate, so that it can be routed for immediate processing (As the message delivery will be delayed for the appropriate time).
                queueMessage.UserProperties.Add("actionType", "immediate");
            } else
            {
                queueMessage.UserProperties.Add("actionType", topicActionObject.ActionType);
            }

            return Task.CompletedTask;
        }

    }
}
