using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using CloudWithChris.Integrations.Approvals.Models;
using System.Collections.Generic;
using System.Linq;

namespace CloudWithChris.Integrations.Approvals.Functions
{
    public static class GetContent
    {
        [FunctionName("GetContent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "content")] HttpRequest req,
            [Table("content", Connection = "IntegrationStoreConnection")] CloudTable cloudTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            TableQuery<ContentObjectTable> tableQuery = new TableQuery<ContentObjectTable>();
            TableQuerySegment<ContentObjectTable> segment = await cloudTable.ExecuteQuerySegmentedAsync(tableQuery, null);
            var data = segment.Select(ContentObjectExtensions.ToContentObject);

            return new OkObjectResult(JsonConvert.SerializeObject(data));
        }
    }
}
