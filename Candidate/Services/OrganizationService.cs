using Microsoft.Data.SqlClient;
using System.Text.Json;
using Candidate.Models; // Import the namespace where Org and other related types are defined

namespace Services
{
    public class OrganizationServices
    {
        private readonly IConfiguration _configuration;

        public OrganizationServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CreateOrganizationTable()
        {
            // Check to see if table exist and add it if it doesn't this is just for this project to make the reviewer life easier
            bool tableExists = false;
            string tableName = "organization";
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    connection.Open();

                    string checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName;";
                    using (SqlCommand command = new SqlCommand(checkTableSql, connection))
                    {
                        command.Parameters.AddWithValue("@TableName", tableName);
                        tableExists = (int)command.ExecuteScalar() > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log the exception
                    Console.WriteLine("Error checking table existence: " + ex.Message);
                }
            }

            // If the table doesn't exist, create it
            if (!tableExists)
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    try
                    {
                        connection.Open();

                        string createTableSql = @"
                    CREATE TABLE " + tableName + @" (
                    id INT PRIMARY KEY,
                    name NVARCHAR(100),
                    minAge INT,
                    questions NVARCHAR(MAX)
                    );";

                        using (SqlCommand command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle or log the exception
                        Console.WriteLine("Error creating table: " + ex.Message);
                    }
                }
            }
        }

        public void CreateOrganization(List<Org> orgList)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

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
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error creating Organization: " + ex.Message);
            }

        }


        public List<Org> GetAllOrganizations()
        {
            // Retrieve top 100 organizations from the database and return them
            try
            {
                List<Org> allOrganizations = new List<Org>();

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string queryString = "SELECT TOP 100 * FROM organization;";

                    using (SqlCommand command = new SqlCommand(queryString, connection))
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
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
                                };

                                allOrganizations.Add(organization);
                            }
                        }
                    }

                }

                return allOrganizations;
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error unable to retrieve organization from database: " + ex.Message);
                return new List<Org>();
            }

        }

        public Org GetOrganization(int id)
        {
            try
            {
                Org organization = null;

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
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
                                    Questions = JsonSerializer.Deserialize<List<bool>>(reader.GetString(reader.GetOrdinal("questions")))
                                };
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

            public List<Candidate.Models.Candidate> GetQualifiedCandidates(int id)
        {
             
            Org organization = GetOrganization(id);
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
