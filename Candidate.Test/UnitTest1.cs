using Xunit;
using System.Collections.Generic;
using Candidate.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services;

namespace Candidate.Tests
{
    public class CandidateControllerTests
    {
        [Fact]
        public async Task Candidates_Returns_OK_WithCandidates()
        {
            // Arrange
            IConfiguration configurationStub = new ConfigurationBuilder().Build(); // You can use a simple stub for IConfiguration
            CandidateServicesStub candidateServiceStub = new CandidateServicesStub(); // Creating a simple stub for CandidateServices

            var controller = new CandidateController(configurationStub, candidateServiceStub);

            // Act
            var result = await controller.Candidates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var candidates = Assert.IsAssignableFrom<List<Models.Candidate>>(okResult.Value);
            Assert.Empty(candidates); // Assuming an empty list is returned
        }

        [Fact]
        public async Task Candidates_Returns_NotFound_WhenNoCandidates()
        {
            // Arrange
            IConfiguration configurationStub = new ConfigurationBuilder().Build(); // You can use a simple stub for IConfiguration
            CandidateServicesStub candidateServiceStub = new CandidateServicesStub { Candidates = null }; // Simulating no candidates found

            var controller = new CandidateController(configurationStub, candidateServiceStub);

            // Act
            var result = await controller.Candidates();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No Candidate Found", notFoundResult.Value);
        }

        // Simple stub for CandidateServices
        public class CandidateServicesStub : CandidateServices
        {
            public List<Models.Candidate> Candidates { get; set; }

            public override Task<List<Models.Candidate>> GetCandidateAsync(int? org = null, int? id = null)
            {
                return Task.FromResult(Candidates);
            }
        }
    }
}
