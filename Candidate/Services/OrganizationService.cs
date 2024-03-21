using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Candidate.Models; // Import the namespace where Org and other related types are defined
using Microsoft.Extensions.Configuration; // Import IConfiguration namespace

namespace Services
{
    public class OrganizationServices
    {
        private readonly IConfiguration _configuration;

        public OrganizationServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? ConnectionString()
        {
            // Database initialization logic goes here
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            return connectionString;
        }

        public List<Org> GetAllOrganizations()
        {
            // Retrieve all organizations from the database and return them
           
            //Variable to save a list of Organizations
            List<Org> allOrganizations = new List<Org>();

            //Connect to the database
            using (SqlConnection connection = new SqlConnection(ConnectionString()))
            {
                //open connection
                connection.Open();

                //create sql command
                string queryString = "SELECT TOP 100 * FROM organization;";

                //run sql command
                using(SqlCommand command = new SqlCommand(queryString, connection))
                {
                    //read the command
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

                            allOrganizations.Add(organization);
                        }
                    }
                }

            }

            return allOrganizations;
        }

        public Org GetOrganization(int id)
        {
            
            Org organization = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString()))
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

            return organization;
        }

        public List<Candidate.Models.Candidate> GetQualifiedCandidates(int id)
        {
            Org organization = GetOrganization(id);
            if (organization == null)
            {
                // Organization not found
                return null;
            }
            else
            {
                return FetchQualifiedCandidates(organization);
            }
        }

        private List<Candidate.Models.Candidate> FetchQualifiedCandidates(Org organization)
        { 
            List<Candidate.Models.Candidate> queryResult = new List<Candidate.Models.Candidate>();

            using (SqlConnection connection = new SqlConnection(ConnectionString()))
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
                            Candidate.Models.Candidate candidate = new Candidate.Models.Candidate
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
                return queryResult;
            }
            
        }
    }
}
