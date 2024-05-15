using Microsoft.Data.SqlClient;
using System.Text.Json;
using Candidate.Models; // Import the namespace where Org and other related types are defined
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class OrganizationServices
    {
        private readonly IConfiguration _configuration;
        private const string DBconnect = "DefaultConnection";
        private readonly string _connectionString;


        public OrganizationServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString(DBconnect);
        }

        private Org MapToOrg(SqlDataReader reader)
        {
            return new Org
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                MinAge = reader.GetInt32(reader.GetOrdinal("minAge")),
                Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
            };
        }

        public async Task<Org> GetOrganization(int id)
        {
            try
            {
                Org organization = null;

                using (SqlConnection connection = new SqlConnection(_connectionString))
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
                                organization = MapToOrg(reader);
                            }
                        }
                    }
                }

                return organization;
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve organizations from database: " + ex.Message);
                return new Org();

            }
        }

        public async Task<List<Org>> GetOrganizationsAsync(int? id = null)
        {
            // Retrieve top 100 organizations from the database and return them
            try
            {
                List<Org> allOrganizations = new List<Org>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string queryString = "SELECT * FROM organization";

                    if (id.HasValue)
                    {
                        queryString += " WHERE id = @id;";
                    }
                    else
                    {
                        queryString = "SELECT TOP 100 * FROM organization;";
                    }


                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        if (id.HasValue)
                        {
                            command.Parameters.AddWithValue("@id", id.Value);
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Org organization = MapToOrg(reader);

                                allOrganizations.Add(organization);
                            }
                        }
                    }
                    return allOrganizations;
                }
                
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve organization from database: " + ex.Message);
                return new List<Org>();
            }

            

        }

        public async Task<string> CreateOrganizationAsync(List<Org> orgList)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var organization in orgList)
                    {
                        string insertDataSql = @"
                                INSERT INTO organization (id, name, minAge, questions)
                                VALUES (@id, @name, @minAge, @questions);
                            ";

                        using (SqlCommand command = new SqlCommand(insertDataSql, connection))
                        {
                            command.Parameters.AddWithValue("@id", organization.Id);
                            command.Parameters.AddWithValue("@name", organization.Name);
                            command.Parameters.AddWithValue("@minAge", organization.MinAge);
                            command.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(organization.Questions));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                return "Organization Create Succesfully";
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error creating Organization: " + ex.Message);
                return "Organization Creation Failed";
            }

        }

        public async Task<string> UpdateOrganizationAsync(int id, Org organization)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if the candidate with the given ID exists
                    string selectCandidateSql = "SELECT * FROM organization WHERE id = @id";

                    using (SqlCommand selectCommand = new SqlCommand(selectCandidateSql, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (!reader.Read())
                            {
                                return "Organization not found";
                            }
                        }
                    }

                    // Update the candidate's information
                    string updateDataSql = @"
                                Update organization
                                SET id = @id, name = @name, minAge = @minAge, questions = @questions
                                Where id = @id;
                            ";

                    using (SqlCommand updateCommand = new SqlCommand(updateDataSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@id", id);
                        updateCommand.Parameters.AddWithValue("@name", organization.Name);
                        updateCommand.Parameters.AddWithValue("@minAge", organization.MinAge);
                        updateCommand.Parameters.AddWithValue("@questions", JsonSerializer.Serialize(organization.Questions));

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return "Organization updated successfully";
                        }
                        else
                        {
                            return "Failed to update organization";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error updating organization: " + ex.Message);
                return "Error updating organization: " + ex.Message;
            }
        }



        public async Task<string> DeleteOrgAsync(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string deleteDataSql = "DELETE FROM organization WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(deleteDataSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return "Organization deleted successfully";
                        }
                        else
                        {
                            return "Organization not found";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error deleting organization from database: " + ex.Message);
                return null;
            }
        }

        public async Task<List<Candidate.Models.Candidate>> GetQualifiedCandidatesAsync(int id)
        {

            Org organization = await GetOrganization(id);
            if (organization == null)
            {
                // Organization not found
                return new List<Candidate.Models.Candidate>();
            }
            else
            {

                return FetchQualifiedCandidates(organization);

            }
        }

        private List<Candidate.Models.Candidate> FetchQualifiedCandidates(Org organization)
        {
            try
            {
                List<Candidate.Models.Candidate> queryResult = new List<Candidate.Models.Candidate>();

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string selectCandidatesSql = @"
                        SELECT * 
                        FROM candidate 
                        WHERE Age >= @MinAge 
                        AND Questions = @Questions 
                        AND EXISTS (
                            SELECT 1
                            FROM OPENJSON(Orgs) WITH (OrgId int '$') AS Org
                            WHERE Org.OrgId = @Id
                        );
                    ";

                    using (SqlCommand command = new SqlCommand(selectCandidatesSql, connection))
                    {
                        command.Parameters.AddWithValue("@MinAge", organization.MinAge);
                        command.Parameters.AddWithValue("@Questions", JsonSerializer.Serialize(organization.Questions));
                        command.Parameters.AddWithValue("@ID", organization.Id);

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
            catch (Exception ex)
            {
                Console.WriteLine("Error unable to retrieve candidate from database: " + ex.Message);
                return new List<Candidate.Models.Candidate>();
            }
        }
    }
}
