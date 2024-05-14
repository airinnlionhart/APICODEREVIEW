using Microsoft.Data.SqlClient;
using System.Text.Json;
using Candidate.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Services
{
    public class CandidateServices
    {
        private const string Name = "DefaultConnection";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        

        public CandidateServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString(Name);
            
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


        public async Task<Candidate.Models.Candidate> GetCandidateAsync(int id)
        {
            try
            {
                Candidate.Models.Candidate candidate = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectDataSql = "SELECT * FROM candidate WHERE id = @id;";

                using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                                candidate = MapToCandidate(reader);
                        }
                    }
                }
                return candidate;
            }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve candidate from database: " + ex.Message);
                return null;
            }
        }

        public async Task<List<Candidate.Models.Candidate>> GetAllCandidateAsync()
        {
            try
            {
                List<Candidate.Models.Candidate> queryResult = new List<Candidate.Models.Candidate>();


                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectDataSql = "SELECT TOP 100 * FROM candidate";

                    using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                    {

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
    }
}

