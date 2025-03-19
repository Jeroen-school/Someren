using Microsoft.Data.SqlClient;
using Someren.Models;
using System.Data;

namespace Someren.Repositories
{
    public class DbLecturersRepository : ILecturersRepository
    {
        //fields and properties
        private readonly string? _connectionString;

        //constructors
        public DbLecturersRepository(IConfiguration configuration)
        {
            //get the connection string from the appsettings
            _connectionString = configuration.GetConnectionString("SomerenDB");
        }

        //methods

        //returns a list of all NOT DELETED lecturers
        public List<Lecturer> GetAll()
        {
            List<Lecturer> lecturers = new List<Lecturer>();
            
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [Deleted] = 0 ORDER BY [last_name]";         //the sql query to execute
            
            ExecuteQuery(lecturers, query);
            
            return lecturers;
        }

        //returns a list of all lecturers where the LAST NAME CONTAINS THE SEARCHED STRING
        public List<Lecturer> GetFiltered(string lastName)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName AND [Deleted] = 0 ORDER BY [last_name]";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@LastName", $"%{lastName}%");

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Lecturer lecturer = ReadLecturer(reader);
                    if (!lecturer.Deleted)
                    {
                        lecturers.Add(lecturer);
                    }
                }
            }
            return lecturers;
        }


        //Search a lecturer by ID, then return a class with the information of that lecturer
        public Lecturer? GetById(int lecturerId)
        {
            Lecturer lecturer = new Lecturer();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [lecturer_id] = @Id";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", lecturerId);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lecturer = ReadLecturer(reader);
                }

                reader.Close();
            }


            return lecturer;
        }


        //ADD a lecturer to the database
        public void Add(Lecturer lecturer)
        {
            //Check if the lecturer has been soft deleted in the past, if they have been, undelete them
            Lecturer? checkIfDeleted = GetById(lecturer.LecturerId);

            if (checkIfDeleted.LecturerId == lecturer.LecturerId && checkIfDeleted.FirstName == lecturer.FirstName && checkIfDeleted.LastName == lecturer.LastName && checkIfDeleted.Deleted)
            {

                string query = $"UPDATE lecturer SET [telephone_number] = @PhoneNumber, [age] = @Age, [Deleted] = 0 WHERE lecturer.lecturer_id = @Id;";

                ExecuteQuery(lecturer, query);
            }
            else
            {
                //If the lecturer is new, create a new lecturer in the database
                string query = $"INSERT INTO lecturer ([lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty])" +
                    $"VALUES (@Id, @RoomNumber, @Firstname, @LastName, @PhoneNumber, @Age, @BarDuty)";
                ExecuteQuery(lecturer, query);
            }
        }

        //EDIT a lecturer in the database
        public void Update(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET [lecturer_id] = @Id, [room_number] = @RoomNumber, [first_name] = @Firstname, [last_name] = @LastName, [telephone_number] = @PhoneNumber, [age] = @Age, [bar_duty] = @BarDuty WHERE lecturer.[lecturer_id] = @Id;";

            ExecuteQuery(lecturer, query);
        }

        //SOFT DELETE a lecturer from the database
        public void Delete(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET Deleted = 1 WHERE lecturer.lecturer_id = @Id;";

            ExecuteQuery(lecturer , query);
        }


        //This is where I keep the methods that PREVENT REPETITION

        //This is to add parameters to the SQL query to prevent an SQL injection
        private static void AddParametersWithValues(SqlCommand command, Lecturer lecturer)
        {
            command.Parameters.AddWithValue("@Id", lecturer.LecturerId);
            command.Parameters.AddWithValue("@RoomNumber", lecturer.RoomNumber);
            command.Parameters.AddWithValue("@FirstName", lecturer.FirstName);
            command.Parameters.AddWithValue("@LastName", lecturer.LastName);
            command.Parameters.AddWithValue("@PhoneNumber", lecturer.PhoneNumber);
            command.Parameters.AddWithValue("@Age", lecturer.Age);
            command.Parameters.AddWithValue("@BarDuty", lecturer.BarDuty);
        }


        //Reads basic information from the Lecturer database
        private Lecturer ReadLecturer(SqlDataReader reader)
        {
            int lecturerId = (int)reader["lecturer_id"];
            string roomNumber = (string)reader["room_number"];
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string phoneNumber = (string)reader["telephone_number"];
            int age = (int)reader["age"];
            bool barDuty = (bool)reader["bar_duty"];
            bool deleted = (bool)reader["Deleted"];

            return new Lecturer(lecturerId, roomNumber, firstName, lastName, phoneNumber, age, barDuty, deleted);
        }

        //To execute the read methods
        private void ExecuteQuery(List<Lecturer> lecturers, string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Lecturer lecturer = ReadLecturer(reader);
                    lecturers.Add(lecturer);
                }

                reader.Close();
            }

        }

        //To execute the Create, Update, and Delete methods
        private void ExecuteQuery(Lecturer lecturer, string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {

                SqlCommand command = new SqlCommand(query, connection);

                AddParametersWithValues(command, lecturer);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Close();
            }

        }


    }
}
