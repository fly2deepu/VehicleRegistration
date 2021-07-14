using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using VehicleRegistrationAPI.Models;
using Microsoft.Extensions.Logging;


namespace VehicleRegistrationAPI
{
    public static class VehicleRegistrationAPI
    {
        //List to store vehicle details
        public static readonly List<VehicleDetails> vehicleDetailsList = new List<VehicleDetails>();

        [FunctionName("NewVehicle")]
        public static async Task<IActionResult> NewVehicle(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "NewVehicle")] HttpRequest req,
            ILogger log)
        {
            try
            {
                
                log.LogInformation("NewVehicle HTTP trigger function processed a request.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var vehicleDetails = JsonConvert.DeserializeObject<VehicleDetails>(requestBody);

                //Check whether vehicle no is of pattern 3 alpha and 3 numeric characters 
                if (!Regex.IsMatch(vehicleDetails.VehicleNo, "^[A-Z]{3}[0-9]{3}$"))
                {
                    return new BadRequestObjectResult("Invalid vehicle number. Please enter valid vehicle number");
                }

                //Check year is between 1960 and 2021 
                if (!Enumerable.Range(1960, 62).Contains(vehicleDetails.Year))
                {
                    return new BadRequestObjectResult(
                        "Invalid vehicle Year. Please enter vehicle year between 1960 and 2021");
                }

                //Check whether vehicle is maximum length of 10
                if (vehicleDetails.Make.Length > 10)
                {
                    return new BadRequestObjectResult("Vehicle make should not exceed more than 10 characters");
                }

                vehicleDetailsList.Add(vehicleDetails);

                return new OkObjectResult("Created ");
            }
            catch (Exception ex)
            {
                log.LogError($"The error occurred while processing GetAllVehicle: {ex.Message}", ex);
                return new InternalServerErrorResult();
            }
        }


        [FunctionName("GetAllVehicles")]
        public static async Task<IActionResult> GetAllVehicles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllVehicles")]  HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("GetAllVehicles HTTP trigger function processed a request.");
                if (vehicleDetailsList.Any())
                {
                    return new OkObjectResult(vehicleDetailsList);
                }
                else
                {
                    return new NotFoundObjectResult("No Vehicle has been register yet");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"The error occurred while processing GetAllVehicle: {ex.Message}", ex);
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("GetVehicleDetails")]
        public static async Task<IActionResult> GetVehicleDetail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetVehicleDetails/{VehicleNumber}")] HttpRequest req,
            ILogger log, string VehicleNumber)
        {
            try
            {
                log.LogInformation("GetVehicleDetails HTTP trigger function processed a request.");

                //Check whether vehicle no is of pattern 3 alpha and 3 numeric characters 
                if (!Regex.IsMatch(VehicleNumber, "^[A-Z]{3}[0-9]{3}$"))
                {
                    return new BadRequestObjectResult("Invalid vehicle number. Please enter valid vehicle number");
                }

                //Extract the first or default vehicle from vehicleDetailsList
                var vehicleDetails = vehicleDetailsList.FirstOrDefault(t => t.VehicleNo == VehicleNumber);
                if (vehicleDetails == null)
                {
                    return new NotFoundObjectResult("No record found");
                }

                return new OkObjectResult(vehicleDetails);

            }
            catch (Exception ex)
            {

                log.LogError($"The error occurred while processing GetAllVehicle: {ex.Message}", ex);
                return new InternalServerErrorResult();
            }
           
        }
    }
}
