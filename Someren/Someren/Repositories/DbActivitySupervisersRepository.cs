using Microsoft.Data.SqlClient;
using Someren.Models;

namespace Someren.Repositories
{
    public class DbActivitySupervisersRepository : IActivitySupervisersRepository
    {
        //fields and properties
        private readonly string? _connectionString;

        //constructors
        public DbActivitySupervisersRepository(IConfiguration configuration)
        {
            //get the connection string from the appsettings
            _connectionString = configuration.GetConnectionString("SomerenDB");
        }


        //methods

        public List<Lecturer> GetAllSupervising(int activityID)
        {
            const string query = $"SELECT L.[lecturer_id], L.[first_name], L.[Last_name], L.[telephone_number], L.[age] " +
                                    $"FROM supervises AS S " +
                                    $"JOIN lecturer AS L ON L.lecturer_id = S.lecturer_id " +
                                    $"WHERE S.activity_id = @Id AND L.deleted != 1;";

            List<Lecturer> lecturers = ExecuteGetAllLecturers(query, activityID);

            return lecturers;
        }

        public List<Lecturer> GetAllOther(int activityID)
        {
            const string query = $"SELECT L.[lecturer_id], L.[first_name], L.[Last_name], L.[telephone_number], L.[age] " +
                                    $"FROM lecturer AS L " +
                                    $"WHERE L.deleted != 1 AND " +
                                    $"L.lecturer_id NOT IN  (SELECT [lecturer_id] " +
                                                            $"FROM supervises " +
                                                            $"WHERE [activity_id] = @Id);";

            List<Lecturer> lecturers = ExecuteGetAllLecturers(query, activityID);

            return lecturers;
        }

        public void AddSupervising(int lecturerID, int activityID)
        {
            const string query =    $"INSERT INTO supervises " +
                                    $"VALUES (@lecturerID, @activityID);";

            ExecuteAddAndRemoveQuery(query, lecturerID, activityID);
        }

        public void RemoveSupervising(int lecturerID, int activityID)
        {
            const string query =    $"DELETE FROM supervises " +
                                    $"WHERE lecturer_id = @lecturerID AND activity_id = @activityID;";

            ExecuteAddAndRemoveQuery(query, lecturerID, activityID);
        }

        //These methods exist to make the codebase less cluttered
        private List<Lecturer> ExecuteGetAllLecturers(string query, int activityID)
        {
            List<Lecturer> lecturers = new();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", activityID);

                command.Connection.Open();

                SqlDataReader read = command.ExecuteReader();

                while (read.Read())
                {
                    lecturers.Add(ReadLecturers(read));
                }
                read.Close();

                return lecturers;
            }
        }

        private void ExecuteAddAndRemoveQuery(string query, int lecturerID, int activityID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@lecturerID", lecturerID);
                command.Parameters.AddWithValue("@activityID", activityID);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Close();
            }
        }

        private Lecturer ReadLecturers(SqlDataReader reader)
        {
            int lecturerId = (int)reader["lecturer_id"];
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string phoneNumber = (string)reader["telephone_number"];
            int age = (int)reader["age"];

            return new Lecturer(lecturerId, firstName, lastName, phoneNumber, age);

        }


    }
}
