using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Company.Function
{
    public static class WebhookDataReceiver
    {
        [FunctionName("WebhookDataReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            log.LogInformation("Function triggered by webhook post request...");
           

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("extracting data stream");
            JObject harvestJson = JsonConvert.DeserializeObject<JObject>(requestBody);
             log.LogInformation("harvester initialized");
            log.LogInformation($"feeding harvester with gathered data");

            if (harvestJson == null)
            {
                log.LogError("data stream did not locate any JSON outcome");
                return new JsonResult(new {error = "data stream did not locate any JSON outcome"});
            }

            if (harvestJson["id"] == null)
            {
                harvestJson["id"] = Guid.NewGuid().ToString();
            }
            if (harvestJson["tenantId"] == null)
            {
                harvestJson["tenantId"] = Guid.NewGuid().ToString();
            }

            log.LogInformation($"harvester belly: {requestBody}");

            log.LogInformation($"starting CosmosDB process...");
        

            string connectionStringURL = "https://kevinweigel.documents.azure.com:443/";
            log.LogInformation($"setting up connection URL");
            log.LogInformation("Gathering secret data");
            string primaryKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            log.LogInformation("Gathered secret data...");
            CosmosClient cosmosClient = new CosmosClient(connectionStringURL, primaryKey);
            log.LogInformation("creating harvesterClient...");
            Container container = cosmosClient.GetContainer(
                "HarvestedData",
                "HarvestedDataContainer"
            );


            try
            {
                if (harvestJson["tenantId"] == null)
                {
                    log.LogError("There is no PartitionKey delivered");
                    return new JsonResult(new { error = "There is no PartitionKey delivered"});
                }


                log.LogInformation("Sending harvestClient to Cosmos....");
                ItemResponse<JObject> response = await container.CreateItemAsync(
                    harvestJson,
                    new PartitionKey(harvestJson["tenantId"].ToString())
                );
                log.LogInformation($"harvester is now in CosmosDB");
                log.LogInformation("Thank you for using CampusEcoRival API");
                return new JsonResult(new{message="harvester is now in CosmosDB", id = response.Resource["id".ToString()]} );
            
            }
            catch (Exception ex)
            {
                log.LogError($"harvester couldn't get sended to Cosmos: {ex}");
                return new JsonResult(new {error =$"harvester couldn't get sended to Cosmos: {ex.Message}"}) {StatusCode = StatusCodes.Status500InternalServerError};
            }
        }
    }
}
