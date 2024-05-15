using Microsoft.Data.SqlClient;
using System.Text.Json;
using Candidate.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Matching;

namespace Services
{
    public class CandidateServices
    {
        private const string DBconnect = "DefaultConnection";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        public CandidateServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString(DBconnect);

        }


        private Candidate.Models.Candidate MapToCandidate(SqlDataReader reader)
        {
            return new Candidate.Models.Candidate
            {
                Id = Convert.ToInt32(reader["id"]),
                Name = reader["name"].ToString(),
                Age = Convert.ToInt32(reader["age"]),
                Orgs = JsonSerializer.Deserialize<List<int>>(reader["orgs"].ToString()),
                Questions = JsonSerializer.Deserialize<List<bool>>(reader["questions"].ToString())
            };
        }


        public async Task<List<Candidate.Models.Candidate>> GetCandidateAsync(int? org = null, int? id = null)
        {
            try
            {
                List<Candidate.Models.Candidate> queryResult = new List<Candidate.Models.Candidate>();


                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectDataSql = "SELECT TOP 100 * FROM candidate";

                    if (org.HasValue)
                    {
                        selectDataSql = @"
                            SELECT TOP 100
                            FROM candidate
                            WHERE EXISTS (
                                SELECT 1
                                FROM OPENJSON(orgs) AS org
                                WHERE org.value = @org
                                );";
                    }

                    if (id.HasValue)
                    {
                        selectDataSql += " WHERE id = @id;";
                    }


                    using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                    {
                        //If orgId is specified, set its value
                        if (org.HasValue)
                        {
                            command.Parameters.AddWithValue("@org", org.ToString());
                        }

                        if (id.HasValue)
                        {
                            command.Parameters.AddWithValue("@id", id.Value);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Candidate.Models.Candidate candidate = MapToCandidate(reader);
                                queryResult.Add(candidate);
                            }
                        }
                    }
                    return queryResult;
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve candidate from database: " + ex.Message);
                return null;
            }
        }

        public async Task<string> CreateCandidateAsync(List<Candidate.Models.Candidate> candidates)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    foreach (Candidate.Models.Candidate candidate in candidates)
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

                return "Candidate created successfully";


            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve candidate from database: " + ex.Message);
                return null;
            }
        }

        public async Task<string> UpdateCandidateAsync(int id, Candidate.Models.Candidate candidate)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if the candidate with the given ID exists
                    string selectCandidateSql = "SELECT * FROM candidate WHERE id = @id";

                    using (SqlCommand selectCommand = new SqlCommand(selectCandidateSql, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (!reader.Read())
                            {
                                return "Candidate not found";
                            }
                        }
                    }

                    // Update the candidate's information
                    string updateDataSql = @"
                UPDATE candidate 
                SET name = @name, age = @age, orgs = @orgs, questions = @questions
                WHERE id = @id;
            ";

                    using (SqlCommand updateCommand = new SqlCommand(updateDataSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@id", id);
                        updateCommand.Parameters.AddWithValue("@name", candidate.Name);
                        updateCommand.Parameters.AddWithValue("@age", candidate.Age);
                        updateCommand.Parameters.AddWithValue("@orgs", JsonSerializer.Serialize(candidate.Orgs));
                        updateCommand.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(candidate.Questions));

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return "Candidate updated successfully";
                        }
                        else
                        {
                            return "Failed to update candidate";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error updating candidate: " + ex.Message);
                return "An error occurred while updating the candidate";
            }
        }


        public async Task<string> DeleteCandidateAsync(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string deleteDataSql = "DELETE FROM candidate WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(deleteDataSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return "Candidate deleted successfully";
                        }
                        else
                        {
                            return "Candidate not found";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error deleting candidate from database: " + ex.Message);
                return null;
            }
        }



    }
}

