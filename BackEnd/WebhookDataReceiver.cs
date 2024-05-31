using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        private static string secretKey = Environment.GetEnvironmentVariable("HmacSecretKey");

        [FunctionName("WebhookDataReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function triggered by webhook post request...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string receivedSignature = req.Headers["X-HMAC-Signature"];

            log.LogInformation($"Received HMAC Signature: {receivedSignature}");
            log.LogInformation($"Request Body: {requestBody}");

            if (string.IsNullOrEmpty(receivedSignature))
            {
                log.LogError("No HMAC signature provided.");
                return new UnauthorizedResult();
            }

            log.LogInformation("signature is not null");

            if (!IsValidHmacSignature(requestBody, receivedSignature, log))
            {
                log.LogError("Invalid HMAC signature.");
                return new UnauthorizedResult();
            }

            log.LogInformation("Is valid Signature");

            log.LogInformation("Extracting data stream");
            JObject harvestJson;
            try
            {
                harvestJson = JsonConvert.DeserializeObject<JObject>(requestBody);
                log.LogInformation("Successfully deserialized JSON request body.");
            }
            catch (JsonException ex)
            {
                log.LogError($"Failed to deserialize JSON: {ex.Message}");
                return new BadRequestObjectResult(new { error = "Invalid JSON format." });
            }

            if (harvestJson == null)
            {
                log.LogError("No JSON data found in the request body.");
                return new BadRequestObjectResult(new { error = "No JSON data found." });
            }

            if (harvestJson["id"] == null)
            {
                harvestJson["id"] = Guid.NewGuid().ToString();
            }
            if (harvestJson["tenantId"] == null)
            {
                harvestJson["tenantId"] = Guid.NewGuid().ToString();
            }

            log.LogInformation($"Processed JSON data: {harvestJson.ToString()}");

            log.LogInformation($"Starting CosmosDB process...");

            string connectionStringURL = "https://kevinweigel.documents.azure.com:443/";
            log.LogInformation($"Setting up connection URL");
            log.LogInformation("Gathering secret data");
            string primaryKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            if (string.IsNullOrEmpty(primaryKey))
            {
                log.LogError("Cosmos DB primary key is not set in the environment variables.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            log.LogInformation("Cosmos DB primary key is set.");
            CosmosClient cosmosClient = new CosmosClient(connectionStringURL, primaryKey);
            log.LogInformation("CosmosClient created.");
            Container container = cosmosClient.GetContainer("HarvestedData", "HarvestedDataContainer");

            try
            {
                if (harvestJson["tenantId"] == null)
                {
                    log.LogError("There is no PartitionKey delivered");
                    return new BadRequestObjectResult(new { error = "There is no PartitionKey delivered" });
                }

                log.LogInformation("Sending harvestClient to Cosmos....");
                ItemResponse<JObject> response = await container.CreateItemAsync(
                    harvestJson,
                    new PartitionKey(harvestJson["tenantId"].ToString())
                );
                log.LogInformation($"Harvester is now in CosmosDB");
                log.LogInformation("Thank you for using CampusEcoRival API");
                return new JsonResult(
                    new
                    {
                        message = "Harvester is now in CosmosDB",
                        id = response.Resource["id"].ToString()
                    }
                );
            }
            catch (CosmosException ex)
            {
                log.LogError($"Failed to save data to Cosmos DB: {ex.Message}");
                log.LogError($"Cosmos DB Status Code: {ex.StatusCode}");
                log.LogError($"Cosmos DB Error Message: {ex.Message}");
                return new JsonResult(new { error = $"Failed to save data to Cosmos DB: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception ex)
            {
                log.LogError($"Unexpected error: {ex.Message}");
                return new JsonResult(new { error = $"Unexpected error: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private static bool IsValidHmacSignature(string requestBody, string receivedSignature, ILogger log)
        {
            log.LogInformation($"Validating HMAC signature...");
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
                log.LogInformation("hash computer build");
                var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
                log.LogInformation("converted to bits");

                log.LogInformation($"Computed HMAC Signature: {computedSignature}");
                log.LogInformation($"Received Signature: {receivedSignature}");
                log.LogInformation($"Secret Key: {secretKey}");

                bool isValid = receivedSignature == computedSignature;
                log.LogInformation($"Signature Valid: {isValid}");

                return isValid;
            }
        }
    }
}
