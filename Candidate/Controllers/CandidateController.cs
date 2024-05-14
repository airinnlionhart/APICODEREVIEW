using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Services;

namespace Candidate.Controllers
{
    [ApiController]
    [Route("api/Candidates")]
    public class CandidateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CandidateServices _candidateServics;

        public CandidateController(IConfiguration configuration, CandidateServices candidatesService)
        {
            _configuration = configuration;
            _candidateServics = candidatesService;
        }

        [HttpGet]
        public async Task<IActionResult> Candidates(int? org = null, int?  id = null)
        {
            try
            {

                List<Models.Candidate> candidates = await _candidateServics.GetAllCandidateAsync(org, id);


                if (candidates == null)
                {
                    return NotFound("No Candidate Found");
                }

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> Candidates(List<Models.Candidate> candidates)
        {
            try
            {
                string results = await _candidateServics.CreateCandidateAsync(candidates);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPut]
        public async Task<ActionResult<string>> Candidates(int id, Models.Candidate candidate)
        {
            try
            {
                string result = await _candidateServics.UpdateCandidateAsync(id, candidate);
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
                string results = await _candidateServics.DeleteCandidateAsync(id);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
 
    }
}
