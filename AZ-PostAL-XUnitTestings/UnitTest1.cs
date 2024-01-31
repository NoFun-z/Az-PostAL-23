using Az_PostAL_23.Controllers;
using Az_PostAL_23.Models;
using Az_PostAL_23.Services;
using Azure.Storage.Blobs.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

namespace AZ_PostAL_XUnitTestings
{
    public class ApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithExpectedModel()
        {
            // Use _client to make requests to your API
            var fakeBlobService = A.Fake<IBlobService>(); // Using FakeItEasy or a similar mocking library
            var expectedBlobs = new List<Transaction> (1);

            // Setup the fake to return the expected blobs when GetAllBlobsWithUri is called
            A.CallTo(() => fakeBlobService.GetAllBlobsWithUri()).Returns(Task.FromResult(expectedBlobs));

            var controller = new TransactionsController(fakeBlobService);

            // Act
            var result = await controller.Index();

            if (result is NotFoundResult)
            {
                var notFoundResult = result as NotFoundResult;
            }
            else if (result is OkObjectResult)
            {
                // This means the status code would be 200
                var okResult = result as OkObjectResult;
                // Perform assertions on the OK result
                Assert.Equal(expectedBlobs.Count, ((List<Transaction>)okResult.Value).Count);
            }
        }
    }
}