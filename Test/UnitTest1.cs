using Microsoft.AspNetCore.Mvc;
using Candidate.Controllers;
using Candidate.Models;
using Services;
using static Candidate.Controllers.CandidateController;
using Microsoft.Extensions.Configuration;

namespace Test;

[TestClass]
public class UnitTest
{
    private IConfiguration configuration;
    private CandidateServices _candidateService;
   
    [TestInitialize]
    public void Initialize()
    {
        // Initialize IConfiguration (use mock or actual configuration)
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json");
        configuration = configBuilder.Build();

        // Initialize CandidateServices (use mock or actual service)
        _candidateService = new CandidateServices(configuration);
    }

    [TestMethod]
    public async Task TestMethod()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);

            // Act
            var result = await controller.Candidates(id: 1234).ConfigureAwait(false) as ObjectResult;


            // Assert
            if (result.Value is List<Candidate.Models.Candidate> candidateList)
            {
                foreach (var candidate in candidateList)
                {
                    Console.WriteLine(candidate.Name);
                    Assert.AreEqual("Aaron Test", candidate.Name);
                }
            }
            Assert.IsNotNull(result);
            Console.WriteLine(result.Value) ;
            Console.WriteLine(result.StatusCode);
            Console.WriteLine(result.ToString());
            Console.WriteLine("making sure this is working again");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }
}