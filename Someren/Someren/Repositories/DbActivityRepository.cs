using Microsoft.Data.SqlClient;
using System.Diagnostics;

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
                string query = "SELECT activity_type, date, time FROM Activity WHERE Deleted = 0 ORDER BY date, time";
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

    public Someren.Models.Activity GetActivityByType(string activityType)
    {
        Someren.Models.Activity activity = null;
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT activity_type, date, time FROM Activity WHERE activity_type = @activityType AND Deleted = 0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@activityType", activityType);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    activity = ReadActivity(reader);
                }
                reader.Close();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
        return activity;
    }

    public void AddActivity(Someren.Models.Activity activity)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Activity (activity_type, date, time) VALUES (@activityType, @date, @time)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@activityType", activity.Activitytype);
                command.Parameters.AddWithValue("@date", activity.Date);
                command.Parameters.AddWithValue("@time", activity.Time);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
    }

    public void UpdateActivity(Someren.Models.Activity activity, string originalType)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Activity SET activity_type = @newType, date = @date, time = @time WHERE activity_type = @originalType";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@newType", activity.Activitytype);
                command.Parameters.AddWithValue("@date", activity.Date);
                command.Parameters.AddWithValue("@time", activity.Time);
                command.Parameters.AddWithValue("@originalType", originalType);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
    }

    public void DeleteActivity(string activityType)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "Update Activity Set Deleted = 1 Where activity_type = @activityType";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@activityType", activityType);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
    }
    public bool ActivityExists(string activityType)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM Activity WHERE activity_type = @activityType";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@activityType", activityType);
                command.Connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Database Error: " + e.Message);
            throw;
        }
    }
}