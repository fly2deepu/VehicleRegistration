using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace VehicleRegistrationAPITest
{
    public class VehicleRegistrationShould
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        private static Mock<HttpRequest> CreateMockRequest(object body)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            var json = JsonConvert.SerializeObject(body);

            sw.Write(json);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest;
        }

        [Fact]
        public async void SuccessfullyCreateNewVehicle()
        {
            var newVehicleRequest = new VehicleRegistrationAPI.Models.VehicleDetails
            {
                VehicleNo = "DEF345",
	            Year = 2010,
	            Make = "i10"
            };

           Mock<HttpRequest> mockRequest = CreateMockRequest(newVehicleRequest);
           var response = (OkObjectResult) await VehicleRegistrationAPI.VehicleRegistrationAPI.NewVehicle(mockRequest.Object, logger);

           Assert.Matches("Created", response.Value.ToString());

        }

        [Fact]
        public async void ThrowInvalidVehicleErrorWhileCreateNewVehicle()
        {
            var newVehicleRequest = new VehicleRegistrationAPI.Models.VehicleDetails
            {
                VehicleNo = "DE1345",
                Year = 2010,
                Make = "i10"
            };

            Mock<HttpRequest> mockRequest = CreateMockRequest(newVehicleRequest);
            var response = (BadRequestObjectResult)await VehicleRegistrationAPI.VehicleRegistrationAPI.NewVehicle(mockRequest.Object, logger);

            Assert.Matches("Invalid vehicle number. Please enter valid vehicle number", response.Value.ToString());

        }

        [Fact]
        public async void ThrowInvalidYearErrorWhileCreateNewVehicle()
        {
            var newVehicleRequest = new VehicleRegistrationAPI.Models.VehicleDetails
            {
                VehicleNo = "DEF345",
                Year = 2030,
                Make = "i10"
            };

             Mock<HttpRequest> mockRequest = CreateMockRequest(newVehicleRequest);
            var response = (BadRequestObjectResult)await VehicleRegistrationAPI.VehicleRegistrationAPI.NewVehicle(mockRequest.Object, logger);

            Assert.Matches("Invalid vehicle Year. Please enter vehicle year between 1960 and 2021", response.Value.ToString());

        }

        [Fact]
        public async void ThrowInvalidMakeErrorWhileCreateNewVehicle()
        {
            var newVehicleRequest = new VehicleRegistrationAPI.Models.VehicleDetails
            {
                VehicleNo = "DEF345",
                Year = 2010,
                Make = "ToyotoSwifti10"
            };

            Mock<HttpRequest> mockRequest = CreateMockRequest(newVehicleRequest);
            var response = (BadRequestObjectResult)await VehicleRegistrationAPI.VehicleRegistrationAPI.NewVehicle(mockRequest.Object, logger);

            Assert.Matches("Vehicle make should not exceed more than 10 characters", response.Value.ToString());

        }

        [Fact]
        public async void ThrowsNoRecordErrorGetVehicledetail()
        {
            string vehicleNo = "XYZ123";
            Mock<HttpRequest> req = CreateMockRequest("GetVehicleDetail");
            var response = (NotFoundObjectResult)await VehicleRegistrationAPI.VehicleRegistrationAPI.GetVehicleDetail(req.Object, logger, vehicleNo);

            Assert.Matches("No record found", response.Value.ToString());

        }

        [Fact]
        public async void ThrowsNoRecordRegisterErrorGetAllVehicles()
        {
            Mock<HttpRequest> req = CreateMockRequest("GetVehicleDetail");
            var response = (NotFoundObjectResult)await VehicleRegistrationAPI.VehicleRegistrationAPI.GetAllVehicles(req.Object, logger);

            Assert.Matches("No Vehicle has been register yet", response.Value.ToString());

        }

    }
   
}
