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
using System.Net;

namespace CloudWithChris.Integrations.Approvals.Functions
{
    public static class Approvals
    {
        [FunctionName("Approvals")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            // Setup the required variables for the Azure Function
            const string PATH = "approvals.html";

            // Find the Approvals.HTML Template
            var scriptPath = Path.Combine(Environment.CurrentDirectory, "www");
            if (!Directory.Exists(scriptPath))
            {
                scriptPath = Path.Combine(
                    Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process),
                    @"site\wwwroot\www");   
            }
            
            // Check if the file exists, return a 404 if it doesn't exist.
            var filePath = Path.GetFullPath(Path.Combine(scriptPath, PATH));
            if (!File.Exists(filePath))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            // Log that the file is being retrieved.
            log.LogInformation($"Attempting to retrieve file at path {filePath}.");

            // Read the file from www/approvals.html and serve it.
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new FileStream(filePath, FileMode.Open))
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");

            return response;
        }
    }
}
