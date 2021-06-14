using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using System;

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
                TableOperation deleteOperation = TableOperation.Delete(new TableEntity("CONTENT", id) { ETag = "*" });
                await cloudTable.ExecuteAsync(deleteOperation);
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
