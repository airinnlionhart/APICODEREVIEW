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



        public async Task<Candidate.Models.Candidate> GetCandidateAsync(int id)
        {
            try
            {
                Candidate.Models.Candidate candidate = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.OpenAsync();

                string selectDataSql = "SELECT * FROM candidate WHERE id = @id;";

                using (SqlCommand command = new SqlCommand(selectDataSql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            candidate = new Candidate.Models.Candidate
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Age = Convert.ToInt32(reader["age"]),
                                Orgs = JsonSerializer.Deserialize<List<int>>(reader["orgs"].ToString()),
                                Questions = JsonSerializer.Deserialize<List<bool>>(reader["questions"].ToString())
                            };
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
    }
}

