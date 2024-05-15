using Microsoft.AspNetCore.Mvc;
using Candidate.Controllers;
using Candidate.Models;
using Services;
using static Candidate.Controllers.CandidateController;
using Microsoft.Extensions.Configuration;
using Organization.Controllers;

namespace Test;

[TestClass]
public class UnitTest
{
    private IConfiguration configuration;
    private CandidateServices _candidateService;
    private OrganizationServices _organizationService;

    [TestInitialize]
    public void Initialize()
    {
        // Initialize IConfiguration (use mock or actual configuration)
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json");
        configuration = configBuilder.Build();

        // Initialize CandidateServices (use mock or actual service)
        _candidateService = new CandidateServices(configuration);
        _organizationService = new OrganizationServices(configuration);
    }
    [TestMethod]
    public async Task CreateCandidate()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);

            List<Candidate.Models.Candidate> candidate = new List<Candidate.Models.Candidate>

                {
                new Candidate.Models.Candidate
                {
                    Id = 0001,
                    Name = "This is for testing",
                    Age = 21,
                    Orgs = new List<int> { 1234, 0001 },
                    Questions = new List<bool> { true, false, true }
                }
            };

            // Act
            var result = await _candidateService.CreateCandidateAsync(candidate);


            Assert.IsNotNull(result);
            Assert.AreEqual("Candidate created successfully", result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }



    [TestMethod]
    public async Task GetCandidate()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);

            // Act
            var result = await controller.Candidates(id: 0001).ConfigureAwait(false) as ObjectResult;


            // Assert
            if (result.Value is List<Candidate.Models.Candidate> candidateList)
            {
                foreach (var candidate in candidateList)
                {
                    Console.WriteLine(candidate.Name);
                    Assert.AreEqual("This is for testing", candidate.Name);
                }
            }
            Assert.IsNotNull(result);
            Console.WriteLine(result.StatusCode);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task EditCandidate()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);

            Candidate.Models.Candidate candidate = new Candidate.Models.Candidate
            {
                Id = 0001,
                Name = "Edited Name",
                Age = 21,
                Orgs = new List<int> { 1234, 0001 },
                Questions = new List<bool> { true, false, true }
            };

            // Act
            var result = await _candidateService.UpdateCandidateAsync(id: 0001, candidate);


            Assert.IsNotNull(result);
            Assert.AreEqual("Candidate updated successfully", result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task GetCandidateChange()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);

            // Act
            var result = await controller.Candidates(id: 0001).ConfigureAwait(false) as ObjectResult;


            // Assert
            if (result.Value is List<Candidate.Models.Candidate> candidateList)
            {
                foreach (var candidate in candidateList)
                {
                    Console.WriteLine(candidate.Name);
                    Assert.AreEqual("Edited Name", candidate.Name);
                }
            }
            Assert.IsNotNull(result);
            Console.WriteLine(result.Value);
            Console.WriteLine(result.StatusCode);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task DeleteCandidate()
    {
        try
        {
            // Arrange
            var controller = new CandidateController(configuration, _candidateService);



            // Act
            var result = await _candidateService.DeleteCandidateAsync(id: 0001);


            Assert.IsNotNull(result);
            Assert.AreEqual("Candidate deleted successfully", result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }


    [TestMethod]
    public async Task CreateOrg()
    {
        try
        {
            // Arrange
            var controller = new OrganizationController(configuration, _organizationService);

            List<Org> organization = new List<Org>

                {
                new Org
                {
                    Id = 0001,
                    Name = "Test Org",
                    MinAge = 16,
                    Questions = new List<bool> { true, false, true }
                }
            };

            // Act
            var result = await _organizationService.CreateOrganizationAsync(organization);
            // Act
            

            Assert.IsNotNull(result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }



    [TestMethod]
    public async Task GetOrganization()
    {
        try
        {
            // Arrange
            var controller = new OrganizationController(configuration, _organizationService);

            // Act
            var result = await controller.Organizations(id: 0001).ConfigureAwait(false) as ObjectResult;
            
            // Assert
            if (result.Value is List<Org> orgList)
            {
                foreach (var org in orgList)
                {
                    Console.WriteLine(org.Name);
                    Assert.AreEqual("Test Org", org.Name);
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task EditOrg()
    {
        try
        {
            // Arrange
            var controller = new OrganizationController(configuration, _organizationService);

            Org organization = new Org
            {
                Id = 0001,
                Name = "Test Put",
                MinAge = 16,
                Questions = new List<bool> { true, false, true }
            };


            // Act
            var result = await _organizationService.UpdateOrganizationAsync(id: 0001, organization);


            Assert.IsNotNull(result);
            Assert.AreEqual("Organization updated successfully", result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task GetOrgChange()
    {
        try
        {
            // Arrange
            var controller = new OrganizationController(configuration, _organizationService);

            // Act
            var result = await controller.Organizations(id: 0001).ConfigureAwait(false) as ObjectResult;

            // Assert
            if (result.Value is List<Org> orgList)
            {
                foreach (var org in orgList)
                {
                    Console.WriteLine(org.Name);
                    Assert.AreEqual("Test Put", org.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }

    [TestMethod]
    public async Task DeleteOrg()
    {
        try
        {
            // Arrange
            var controller = new OrganizationController(configuration, _organizationService);



            // Act
            var result = await _organizationService.DeleteOrgAsync(id: 0001);
            


            Assert.IsNotNull(result);
            Assert.AreEqual("Organization deleted successfully", result);
            Console.WriteLine(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
            throw; // Rethrow the exception to fail the test
        }
    }
}