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
    
    class PayloadCheck 
    {
        public object PayloadIsNull(object payload)
        {
            if (payload == null)
            {

                return new BadRequestObjectResult("The required Payload is null");
            }
            else return new OkObjectResult("Payload received process will proceed");

        }

        public bool QueryAuthorization(string query, string environmentQuery)
        {
            
            if (query != environmentQuery)
            {
                return false;
            }
            return true;
        }

    }
}