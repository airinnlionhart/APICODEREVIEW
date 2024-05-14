
using Microsoft.AspNetCore.Mvc;
using Candidate.Models;
using Candidate;
using Services;


namespace Organization.Controllers
{
    [ApiController]
    [Route("api/Organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly OrganizationServices _organizationService;
        private readonly IConfiguration _configuration;

        public OrganizationController(IConfiguration configuration, OrganizationServices organizationService)
        {
            _configuration = configuration;
            _organizationService = organizationService;
        }

        [HttpPost("CreateOrg")]
        public IActionResult InitializeDatabase(List<Org> orgList)
        {
            try
            {
                if (orgList == null || !orgList.Any())
                {
                    return BadRequest("No organization provided.");
                }

                // Check to make sure the table exist and create it if not
                _organizationService.CreateOrganizationTable();

                // Insert organizations into the database
                _organizationService.CreateOrganization(orgList);

                return Ok("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetAllOrgs")]
        public IActionResult GetAllOrganizations()
        {
            try
            {
                List<Org> getAllOrganizations = _organizationService.GetAllOrganizations();

                if (getAllOrganizations == null)
                {
                   return NotFound("Organization not found.");
                }
                return Ok(getAllOrganizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetOrgById/{id}")]
        public IActionResult GetOrganizationById(int id)
        {
            try
            {
               
                Org organization = _organizationService.GetOrganization(id);


                if (organization == null)
                {
                    return NotFound("Organization not found.");
                }

                return Ok(organization);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetOrgById/{id}/qualifiedCandidates")]
        public IActionResult MatchQuestions(int id)
        {
            try
            {
                List<Candidate.Models.Candidate> queryResult = _organizationService.GetQualifiedCandidates(id);
                return Ok(queryResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}