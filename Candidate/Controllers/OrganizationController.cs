
using Microsoft.AspNetCore.Mvc;
using Candidate.Models;
using Candidate;
using Services;
using System.Reflection;


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

        [HttpPost()]
        public async Task<ActionResult<string>> Organizations(List<Org> orgList)
        {
            try
            {
                if (orgList == null || !orgList.Any())
                {
                    return BadRequest("No organization provided.");
                }

                // Insert organizations into the database
                string results = await _organizationService.CreateOrganizationAsync(orgList);

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Organizations(int? id = null)
        {
            try
            {
                List<Org> getAllOrganizations = await _organizationService.GetOrganizationsAsync(id);

                if (getAllOrganizations.Count == 0 && id.HasValue)
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

        [HttpPut]
        public async Task<ActionResult<string>> Organizations(int id, Org organization)
        {
            try
            {
                string result = await _organizationService.UpdateOrganizationAsync(id, organization);
                if (result == "Candidate updated successfully")
                {
                    return Ok(result);
                }
                else if (result == "Candidate not found")
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<ActionResult<string>> DeleteCandidate(int id)
        {
            try
            {
                string results = await _organizationService.DeleteOrgAsync(id);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("{id}/qualifiedCandidates")]
        public async Task<IActionResult> MatchQuestions(int id)
        {
            try
            {
                List<Candidate.Models.Candidate> queryResult = await _organizationService.GetQualifiedCandidatesAsync(id);
                return Ok(queryResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}