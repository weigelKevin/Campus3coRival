using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class WebhookDataReceiver
    {
        
        [FunctionName("WebhookDataReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string envQuery = Environment.GetEnvironmentVariable("");
            string streamQuery = req.Query["code"];

            PayloadCheck p1 = new PayloadCheck();
            
            log.LogInformation("Function got triggered by Webhook request ....");

            bool queryBool = p1.QueryAuthorization(envQuery, streamQuery);
            
            
            if (queryBool!=false)
            {
                log.LogError("Authorization Unsucessfull");
                return new UnauthorizedResult();
            }
            

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var convRequestBody = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation("deserialization of request body ....");
            //p1.PayloadIsNull(convRequestBody);
            log.LogInformation($"{convRequestBody}");


            return new OkObjectResult(convRequestBody);
        }
    }
}
