using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Someren.Models;

namespace Someren.Repositories;
public class DbActivityRepository : IDbActivityRepository
{
    private readonly string? _connectionString;

    public DbActivityRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SomerenDb");
    }

    private Someren.Models.Activity ReadActivity(SqlDataReader reader)
    {
        string activitytype = (string)reader["activity_type"];
        DateTime date = (DateTime)reader["date"];
        TimeSpan time = (TimeSpan)reader["time"];

        return new Models.Activity(activitytype, date, time);
    }

    public List<Someren.Models.Activity> ViewAllActivities()
    {
        var activities = new List<Someren.Models.Activity>();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT activity_type, date, time FROM Activity ORDER BY date, time";
                SqlCommand command = new SqlCommand(query, connection);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Someren.Models.Activity activity = ReadActivity(reader);
                    activities.Add(activity);
                }

                reader.Close();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
        return activities;
    }
}

