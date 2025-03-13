using Someren.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Someren.Repositories
{
    public class StudentsRepository : IStudentsRepository
    {
        private readonly string? _connectionString;


        public StudentsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SomerenDB");
        }

        private Student ReadStudent(SqlDataReader reader)
        {
            int studentNum = (int)reader["student_number"];
            string roomNum = (string)reader["room_number"];
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string telNum = (string)reader["telephone_number"];
            string studentClass = (string)reader["class"];

            return new Student(studentNum, roomNum, firstName, lastName, telNum, studentClass);
        }

        public List<Student> GetAll()
        {
            List<Student> students = new List<Student>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT [student_number], [room_number], [first_name], [last_name], [telephone_number], [class] FROM student ORDER BY student.last_name;";
                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Student student = ReadStudent(reader);
                    students.Add(student);
                }
                reader.Close();
            }
            return students;
        }

        public Student? GetByNum (int studentNum)
        {
            Student student = new Student();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT [student_number], [room_number], [first_name], [last_name], [telephone_number], [class] FROM student WHERE student.student_number = {student.StudentNum};";
                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    student = ReadStudent(reader);
                }
                reader.Close();
            }
            return student;
        }

        public void Add(Student student)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO student (student_number, room_number, first_name, last_name, telephone_number, class)" +
                                "VALUES (@StudentNum, @RoomNum, @FirstName, @LastName, @TelNum, @Class);";
                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@StudentNum", student.StudentNum);
                cmd.Parameters.AddWithValue("@RoomNum", student.RoomNum);
                cmd.Parameters.AddWithValue("@FirstName", student.FirstName);
                cmd.Parameters.AddWithValue("@LastName", student.LastName);
                cmd.Parameters.AddWithValue("@TelNum", student.TelNum);
                cmd.Parameters.AddWithValue("@Class", student.StudentClass);

                cmd.Connection.Open();
            }
        }

        public void Update(Student student)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE student SET room_number = @RoomNum, first_name = @FIrstName, last_name = @LastName, telephone_number = @TelNum, class = @Class" +
                    "WHERE student.student_number = @StudentNum;";
                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@StudentNum", student.StudentNum);
                cmd.Parameters.AddWithValue("@RoomNum", student.RoomNum);
                cmd.Parameters.AddWithValue("@FirstName", student.FirstName);
                cmd.Parameters.AddWithValue("@LastName", student.LastName);
                cmd.Parameters.AddWithValue("@TelNum", student.TelNum);
                cmd.Parameters.AddWithValue("@Class", student.StudentClass);

                cmd.Connection.Open();

                int nrOfRowsAffected = cmd.ExecuteNonQuery();
                if (nrOfRowsAffected == 0)
                {
                    throw new Exception("No records updated!");
                }
            }
        }

        public void Delete(Student student)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Users WHERE UserId = @Id";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StudentNum", student.StudentNum);

                cmd.Connection.Open();

                int rowsAffected = cmd.ExecuteNonQuery();  // Actually executes the DELETE statement

                if (rowsAffected == 0)
                {
                    throw new Exception("No records deleted! User might not exist.");
                }
            }
        }
    }
}
