using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using CloudWithChris.Integrations.Approvals.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace CloudWithChris.Integrations.Approvals.Functions
{
  public static class GetContent
    {
        [FunctionName("GetContent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "content/{modeParameter?}")] HttpRequest req,
            [Table("content", Connection = "IntegrationStoreConnection")] CloudTable cloudTable,
            ILogger log,
            string modeParameter = "0")
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            int mode;
            Int32.TryParse(modeParameter, out mode);

            TableQuery<ContentObjectTable> tableQuery = new TableQuery<ContentObjectTable>();
            TableQuerySegment<ContentObjectTable> segment = await cloudTable.ExecuteQuerySegmentedAsync(tableQuery, null);
            List<ContentObject> data = segment.Select(ContentObjectExtensions.ToContentObject).Where(e => e.Processed == mode).ToList();

            return new OkObjectResult(JsonConvert.SerializeObject(data));
        }
    }
}
