using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using System;
using CloudWithChris.Integrations.Approvals.Models;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace CloudWithChris.Integrations.Approvals.Functions
{
  public static class DeleteContent
    {
        [FunctionName("DeleteContent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "content/{id}")] HttpRequest req,
            [Table("content", Connection = "IntegrationStoreConnection")] CloudTable cloudTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete requested of content {id} in table store.");

            try
            {
                int flag = 0;
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                DeleteModeMessage data = JsonConvert.DeserializeObject<DeleteModeMessage>(requestBody);

                if (data.mode == "approve")
                {
                    flag = 1;
                }
                else if (data.mode == "reject")
                {
                    flag = -1;
                }

                var entity = new DynamicTableEntity("CONTENT", id);
                entity.ETag = "*";
                entity.Properties.Add("Processed", new EntityProperty(flag));
                TableOperation mergeOperation = TableOperation.Merge(entity);

                await cloudTable.ExecuteAsync(mergeOperation);
                return new OkObjectResult("Object deleted");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new NotFoundResult();
            }
        }
    }
}


public class DeleteModeMessage
{
    public string mode { get; set; }
} 