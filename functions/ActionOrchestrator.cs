using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using CloudWithChris.Integrations.Approvals.Models;
using System.Collections.Generic;

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

            List<Task> taskList = new List<Task>();
            
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
                        // 1. Detect any cloudwithchris.com URLs and replace them
                        //    with shortened URLs that navigate to UTM links
                        //    to help with tracking marketing campaigns.
                        // 2. Take the contents of above item, merge with the JSON
                        //    object and submit to the Topic.

                        TopicActionObject topicActionObject = new TopicActionObject()
                        {
                            Id = contentAndActions.Id,
                            Title = contentAndActions.Title,
                            ContentType = contentAndActions.ContentType,
                            Source = contentAndActions.Source,
                            Summary = contentAndActions.Summary,
                            ActionType = contentAndActions.Actions[actionPtr].ActionTypes[actionTypesPtr],
                            Platform = contentAndActions.Actions[actionPtr].Platforms[platformsPtr]
                        };

                        taskList.Add(context.CallSubOrchestratorAsync("TopicActionTransformAndSend", topicActionObject));
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

            // Send the object to the URLShortenerService
            Task<string> shortURL = context.CallActivityAsync<string>("RetrieveShortURL", topicActionObject);

            // Create a combined object, so that we can transform the urls within the summary.
            TopicActionObjectEnvelope topicActionObjectEnvelope = new TopicActionObjectEnvelope(){
                TopicActionObject = topicActionObject,
                ShortURL = await shortURL
            };

            // Update the message contents to include the url
            Task<TopicActionObject> convertedTopicActionObject = context.CallActivityAsync<TopicActionObject>("UpdateMessageWithShortURL", topicActionObjectEnvelope);


            // Send the resulting object to the Service Bus Topic
            Task sentToTopic = context.CallActivityAsync("SendToTopic", convertedTopicActionObject);

            await Task.WhenAll(sentToTopic);
        }

        [FunctionName("RetrieveShortURL")]
        public async static Task<Uri> RetrieveShortURL
        (
            [ActivityTrigger] TopicActionObject topicActionObject,
            ILogger log
        )
        {
            // To Do: Implement the short URL fetcher...
            return new Uri("http://cloudchris.ws");
        }

        [FunctionName("UpdateMessageWithShortURL")]
        public async static Task<TopicActionObject> UpdateMessageWithShortURL
        (
            [ActivityTrigger] TopicActionObjectEnvelope topicActionObjectEnvelope,
            ILogger log
        )
        {
            // To do - implement the functionality to either add (?) or replace (?) urls in the body.
            return topicActionObjectEnvelope.TopicActionObject;
        }

        
        [FunctionName("SendToTopic")]
        public async static Task SendToTopic
        (
            [ActivityTrigger] TopicActionObject topicActionObject,
            ILogger log
        )
        {
            // To do - implement the functionality to TRY and send a message to the queue...
        }

    }
}
