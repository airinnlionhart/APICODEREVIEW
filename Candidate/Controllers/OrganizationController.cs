using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Candidate.Models;
using Microsoft.AspNetCore.Routing.Matching;
using System.Windows.Input;
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

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                // Check if the organization table exists
                bool tableExists;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'organization';";
                    using (SqlCommand command = new SqlCommand(checkTableSql, connection))
                    {
                        tableExists = (int)command.ExecuteScalar() > 0;
                    }
                }

                // If the table doesn't exist, create it
                if (!tableExists)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string createTableSql = @"
                             CREATE TABLE organization (
                             id INT PRIMARY KEY,
                             name NVARCHAR(100),
                             minAge INT,
                             candidateIds NVARCHAR(MAX),
                             questions NVARCHAR(MAX)
                             );
                             ";

                        using (SqlCommand command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // Insert organizations into the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var organization in orgList)
                    {
                        string insertDataSql = @"
                            INSERT INTO organization (id, name, minAge, candidateIds, questions)
                            VALUES (@id, @name, @minAge, @candidateIds, @questions);
                        ";

                        using (SqlCommand command = new SqlCommand(insertDataSql, connection))
                        {
                            command.Parameters.AddWithValue("@id", organization.Id);
                            command.Parameters.AddWithValue("@name", organization.Name);
                            command.Parameters.AddWithValue("@minAge", organization.MinAge);
                            command.Parameters.AddWithValue("@candidateIds", JsonSerializer.Serialize(organization.CandidateIds));
                            command.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(organization.Questions));

                            command.ExecuteNonQuery();
                        }
                    }
                }

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