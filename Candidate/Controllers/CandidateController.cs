using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Candidate.Models;
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
        public IActionResult GetAllCandidates()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                List<Models.Candidate> queryResult = new List<Models.Candidate>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT TOP 100 * FROM candidate;", connection))
                    {
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

                return Ok(queryResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("CreateCandidate")]
        public IActionResult InitializeDatabase(List<Models.Candidate> candidates)
        {
            try
            {
                if (candidates == null || candidates.Count == 0)
                {
                    return BadRequest("No candidates provided.");
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                bool tableExists = false;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'candidate';";
                    using (SqlCommand command = new SqlCommand(checkTableSql, connection))
                    {
                        tableExists = (int)command.ExecuteScalar() > 0;
                    }
                }

                if (!tableExists)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string createTableSql = @"
                             CREATE TABLE candidate (
                             id INT PRIMARY KEY,
                             name NVARCHAR(100),
                             age INT,
                             orgs NVARCHAR(MAX),
                             questions NVARCHAR(MAX)
                             );
                             ";

                        using (SqlCommand command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (Models.Candidate candidate in candidates)
                    {
                        string insertDataSql = @"
                            INSERT INTO candidate (id, name, age, orgs, questions)
                            VALUES (@id, @name, @age, @orgs, @questions);
                        ";

                        using (SqlCommand command = new SqlCommand(insertDataSql, connection))
                        {
                            command.Parameters.AddWithValue("@id", candidate.Id);
                            command.Parameters.AddWithValue("@name", candidate.Name);
                            command.Parameters.AddWithValue("@age", candidate.Age);
                            command.Parameters.AddWithValue("@orgs", JsonSerializer.Serialize(candidate.Orgs));
                            command.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(candidate.Questions));

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
        [HttpPost("DeleteCandidate/{id}")]
        public IActionResult DeleteCandidate(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string deleteCandidateSql = @"
                DELETE FROM candidate
                WHERE id = @id;
            ";

                    using (SqlCommand command = new SqlCommand(deleteCandidateSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound($"Candidate with ID {id} not found.");
                        }
                    }
                }

                return Ok($"Candidate with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetCandidateById(int id)
        {
            try
            {

                Candidate.Models.Candidate candidate = _candidateServics.GetCandidate(id);


                if (candidate == null)
                {
                    return NotFound("No Candidate Found");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetCandidatesByOrg/{org}")]
        public IActionResult GetCandidatesByOrg(int org)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string getCandidatesSql = @"
                SELECT id, name, age, orgs, questions
                FROM candidate
                WHERE EXISTS (
                    SELECT 1
                    FROM OPENJSON(orgs) AS org
                    WHERE org.value = @org
                );
            ";

                    using (SqlCommand command = new SqlCommand(getCandidatesSql, connection))
                    {
                        command.Parameters.AddWithValue("@org", org.ToString());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<Models.Candidate> candidates = new List<Models.Candidate>();

                            while (reader.Read())
                            {
                                var candidate = new Models.Candidate
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Age = reader.GetInt32(2),
                                    Orgs = JsonSerializer.Deserialize<List<int>>(reader.GetString(3)),
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(4))
                                };

                                candidates.Add(candidate);
                            }

                            if (candidates.Count > 0)
                            {
                                return Ok(candidates);
                            }
                            else
                            {
                                return NotFound($"No candidates found for organization {org}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("AddOrgToCandidate/{candidateId}")]
        public IActionResult AddOrgToCandidate(int candidateId, int orgId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                // Check if the candidate exists
                if (!CandidateExists(candidateId, connectionString))
                {
                    return NotFound($"Candidate with ID {candidateId} not found.");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch candidate's existing organizations
                    string getOrgsSql = @"
                SELECT orgs
                FROM candidate
                WHERE id = @candidateId;
            ";

                    List<int> orgs = new List<int>();
                    using (SqlCommand command = new SqlCommand(getOrgsSql, connection))
                    {
                        command.Parameters.AddWithValue("@candidateId", candidateId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orgs = JsonSerializer.Deserialize<List<int>>(reader.GetString(0));
                            }
                        }
                    }

                    // Add the new organization to the list
                    orgs.Add(orgId);

                    // Update candidate's organizations
                    string updateOrgsSql = @"
                UPDATE candidate
                SET orgs = @orgs
                WHERE id = @candidateId;
            ";

                    using (SqlCommand command = new SqlCommand(updateOrgsSql, connection))
                    {
                        command.Parameters.AddWithValue("@candidateId", candidateId);
                        command.Parameters.AddWithValue("@orgs", JsonSerializer.Serialize(orgs));

                        command.ExecuteNonQuery();
                    }
                }

                return Ok($"Organization with ID {orgId} added to candidate with ID {candidateId} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private bool CandidateExists(int candidateId, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string checkCandidateSql = "SELECT COUNT(*) FROM candidate WHERE id = @candidateId;";
                using (SqlCommand command = new SqlCommand(checkCandidateSql, connection))
                {
                    command.Parameters.AddWithValue("@candidateId", candidateId);

                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        [HttpPost("AddQuestionsToCandidate/{candidateId}")]
        public IActionResult AddQuestionsToCandidate(int candidateId, List<bool> questions)
        {
            try
            {
                if (questions == null || questions.Count == 0)
                {
                    return BadRequest("No questions provided.");
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                // Check if the candidate exists
                if (!CandidateExists(candidateId, connectionString))
                {
                    return NotFound($"Candidate with ID {candidateId} not found.");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch candidate's existing questions
                    string getQuestionsSql = @"
                SELECT questions
                FROM candidate
                WHERE id = @candidateId;
            ";

                    List<bool> existingQuestions = new List<bool>();
                    using (SqlCommand command = new SqlCommand(getQuestionsSql, connection))
                    {
                        command.Parameters.AddWithValue("@candidateId", candidateId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingQuestions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(0));
                            }
                        }
                    }

                    // Add the new questions to the existing list
                    existingQuestions.AddRange(questions);

                    // Update candidate's questions
                    string updateQuestionsSql = @"
                UPDATE candidate
                SET questions = @questions
                WHERE id = @candidateId;
            ";

                    using (SqlCommand command = new SqlCommand(updateQuestionsSql, connection))
                    {
                        command.Parameters.AddWithValue("@candidateId", candidateId);
                        command.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(existingQuestions));

                        command.ExecuteNonQuery();
                    }
                }

                return Ok($"Questions added to candidate with ID {candidateId} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



    }
}
