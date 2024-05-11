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
            log.LogInformation("Funktion wurde durch einen Webhook-Aufruf ausgelöst...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("Stream readed");
            dynamic harvestJson = JsonConvert.DeserializeObject<dynamic>(requestBody);

            if (harvestJson == null)
            {
                log.LogError("Anforderungskörper ist leer oder nicht im JSON-Format.");
                return new BadRequestObjectResult(
                    "Anforderungskörper ist leer oder nicht im JSON-Format."
                );
            }

            if (harvestJson.id == null)
            {
                harvestJson.id = Guid.NewGuid().ToString();
            }

            log.LogInformation($"Empfangene Daten: {requestBody}");

            string connectionStringURL = "https://kevinweigel.documents.azure.com:443/";
            string primaryKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            CosmosClient cosmosClient = new CosmosClient(connectionStringURL, primaryKey);
            Container container = cosmosClient.GetContainer(
                "HarvestedData",
                "HarvestedDataContainer"
            );

            try
            {
                if (harvestJson.tenantId == null)
                {
                    log.LogError("Tenant-ID fehlt im Anforderungskörper.");
                    return new BadRequestObjectResult("Tenant-ID fehlt im Anforderungskörper.");
                }

                ItemResponse<dynamic> response = await container.CreateItemAsync(
                    harvestJson,
                    new PartitionKey(harvestJson.tenantId.ToString())
                );
                log.LogInformation($"Element in Cosmos DB mit ID erstellt: {response.Resource.id}");
                return new OkObjectResult($"Element erstellt mit ID: {response.Resource.id}");
            }
            catch (Exception ex)
            {
                log.LogError($"Fehler beim Erstellen des Elements in Cosmos DB: {ex}");
                return new ObjectResult($"Interner Serverfehler: {ex.Message}")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
