using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CloudWithChris.Integrations.Approvals.Functions
{
    public static class PostActionsToTopic
    {
        [FunctionName("HttpStart")]
        // TODO: Change away from anonymous and fix local development scenario for POSTing
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "post", Route = "actions")] HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            try 
            {
                // Function input comes from the request content.
                object eventData = await req.Content.ReadAsAsync<object>();
                string instanceId = await starter.StartNewAsync("ActionOrchestrator", eventData);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                return req.CreateResponse(System.Net.HttpStatusCode.Accepted, $"Started ActionOrchestrator {instanceId}");
            } catch (Exception ex)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
