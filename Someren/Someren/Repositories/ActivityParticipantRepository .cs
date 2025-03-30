using Microsoft.Data.SqlClient;
using Someren.Models;
using System.Diagnostics;

namespace Someren.Repositories
{
    public class ActivityParticipantRepository : IActivityParticipantRepository
    {
        private readonly string? _connectionString;

        public ActivityParticipantRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SomerenDb");
        }

        private Student ReadStudent(SqlDataReader reader)
        {
            // Notice we're using student_number, not student_id as the ID
            int studentNum = (int)reader["student_number"];
            string roomNum = reader["room_id"].ToString(); // Use room_id instead of room_number
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string telNum = (string)reader["telephone_number"];
            string studentClass = (string)reader["class"];

            return new Student(studentNum, roomNum, firstName, lastName, telNum, studentClass);
        }

        public List<Student> GetParticipants(string activityType)
        {
            var participants = new List<Student>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT s.student_id, s.room_id, s.student_number, s.first_name, s.last_name, 
                               s.telephone_number, s.class 
                        FROM student s
                        JOIN participates p ON s.student_id = p.student_id
                        JOIN activity a ON p.activity_id = a.activity_id
                        WHERE a.activity_type = @activityType 
                          AND s.deleted = 0
                          AND a.deleted = 0
                        ORDER BY s.last_name";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@activityType", activityType);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Student student = ReadStudent(reader);
                        participants.Add(student);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Database Error: " + e.Message);
                throw;
            }
            return participants;
        }

        public List<Student> GetNonParticipants(string activityType)
        {
            var nonParticipants = new List<Student>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT s.student_id, s.room_id, s.student_number, s.first_name, s.last_name, 
                               s.telephone_number, s.class 
                        FROM student s
                        WHERE s.deleted = 0 
                        AND s.student_id NOT IN (
                            SELECT p.student_id 
                            FROM participates p
                            JOIN activity a ON p.activity_id = a.activity_id
                            WHERE a.activity_type = @activityType
                              AND a.deleted = 0
                        )
                        ORDER BY s.last_name";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@activityType", activityType);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Student student = ReadStudent(reader);
                        nonParticipants.Add(student);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Database Error: " + e.Message);
                throw;
            }
            return nonParticipants;
        }

        public void AddParticipant(string activityType, int studentNum)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    int activityId = GetActivityIdByType(connection, activityType);
                    int studentId = GetStudentIdByNumber(connection, studentNum);

                    string query = @"
                        INSERT INTO participates (student_id, activity_id) 
                        VALUES (@studentId, @activityId)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@studentId", studentId);
                    command.Parameters.AddWithValue("@activityId", activityId);
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

        public void RemoveParticipant(string activityType, int studentNum)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    int activityId = GetActivityIdByType(connection, activityType);
                    int studentId = GetStudentIdByNumber(connection, studentNum);

                    string query = @"
                        DELETE FROM participates 
                        WHERE student_id = @studentId AND activity_id = @activityId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@studentId", studentId);
                    command.Parameters.AddWithValue("@activityId", activityId);
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

        private int GetActivityIdByType(SqlConnection connection, string activityType)
        {
            string query = "SELECT activity_id FROM activity WHERE activity_type = @activityType AND deleted = 0";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@activityType", activityType);

            bool openedConnection = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                openedConnection = true;
            }

            int activityId = (int)command.ExecuteScalar();

            if (openedConnection)
                connection.Close();

            return activityId;
        }

        private int GetStudentIdByNumber(SqlConnection connection, int studentNum)
        {
            string query = "SELECT student_id FROM student WHERE student_number = @studentNum AND deleted = 0";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentNum", studentNum);

            bool openedConnection = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                openedConnection = true;
            }

            int studentId = (int)command.ExecuteScalar();

            if (openedConnection)
                connection.Close();

            return studentId;
        }
    }
}