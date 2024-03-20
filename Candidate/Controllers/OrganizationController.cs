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

namespace Candidate.Controllers
{
    [ApiController]
    [Route("api/Organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrganizationController(IConfiguration configuration)
        {
            _configuration = configuration;
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
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                List<Org> organizations = new List<Org>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectDataSql = "SELECT * FROM organization;";

                    using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Org organization = new Org
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    MinAge = reader.GetInt32(reader.GetOrdinal("minAge")),
                                    CandidateIds = JsonSerializer.Deserialize<List<int>>(reader.GetString(reader.GetOrdinal("candidateIds"))),
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
                                };

                                organizations.Add(organization);
                            }
                        }
                    }
                }

                return Ok(organizations);
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
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                Org organization = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectDataSql = "SELECT * FROM organization WHERE id = @id;";

                    using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                organization = new Org
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    MinAge = reader.GetInt32(reader.GetOrdinal("minAge")),
                                    CandidateIds = JsonSerializer.Deserialize<List<int>>(reader.GetString(reader.GetOrdinal("candidateIds"))),
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
                                };
                            }
                        }
                    }
                }

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
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                Org organization = null;
                List<Models.Candidate> queryResult = new List<Models.Candidate>();

                // Fetch organization details
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectOrgDataSql = "SELECT MinAge, Questions FROM organization WHERE id = @id;";

                    using (SqlCommand command = new SqlCommand(selectOrgDataSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                organization = new Org
                                {
                                    MinAge = reader.GetInt32(reader.GetOrdinal("MinAge")),
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
                                };
                            }
                        }
                    }
                }

                if (organization == null)
                {
                    return NotFound("Organization not found.");
                }
                else
                {
                    // Fetch qualified candidates based on MinAge and Questions
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string selectCandidatesSql = "SELECT * FROM candidate WHERE Age >= @MinAge AND Questions = @Questions;";

                        using (SqlCommand command = new SqlCommand(selectCandidatesSql, connection))
                        {
                            command.Parameters.AddWithValue("@MinAge", organization.MinAge);
                            command.Parameters.AddWithValue("@Questions", JsonSerializer.Serialize(organization.Questions));

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Models.Candidate candidate = new Models.Candidate
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Name = reader["name"].ToString(),
                                        Age = Convert.ToInt32(reader["age"]),
                                        Orgs = JsonSerializer.Deserialize<List<int>>(reader["orgs"].ToString()),
                                        Questions = JsonSerializer.Deserialize<List<bool>>(reader["questions"].ToString())
                                    };

                                    queryResult.Add(candidate);
                                }
                            }
                        }
                    }
                }

                return Ok(queryResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}