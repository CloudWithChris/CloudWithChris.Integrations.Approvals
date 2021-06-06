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
using System.Net;

namespace CloudWithChris.Integrations.Approvals.Functions
{
    public static class PostActionsToTopic
    {
        [FunctionName("HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "actions")] HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {

            if (req == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var check = SecurityCheck(req);
            if (check != null)
            {
                return check;
            }

            try
            {
                // Function input comes from the request content.
                object eventData = await req.Content.ReadAsAsync<object>();
                string instanceId = await starter.StartNewAsync("ActionOrchestrator", eventData);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                return req.CreateResponse(System.Net.HttpStatusCode.OK, JsonConvert.SerializeObject($"Started orchestration with ID {instanceId}"), "text/json");
            } catch (Exception ex)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        private static HttpResponseMessage SecurityCheck(HttpRequestMessage req)
        {
            return req.IsLocal() || req.RequestUri.Scheme == "https" ? null :
                req.CreateResponse(HttpStatusCode.Forbidden);
        }


        public static bool IsLocal(this HttpRequestMessage request)
        {
            return request.RequestUri.IsLoopback;
        }
    }
}
