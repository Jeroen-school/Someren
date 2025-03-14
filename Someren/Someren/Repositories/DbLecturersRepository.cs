using Microsoft.Data.SqlClient;
using Someren.Models;

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
        public List<Lecturer> GetAll()
        {
            List<Lecturer> lecturers = new List<Lecturer>();
            
            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {
                string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer ORDER BY [last_name]";         //the sql query to execute
                SqlCommand command = new SqlCommand(query, connection);

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Lecturer lecturer = ReadLecturer(reader);
                    if (!lecturer.Deleted)
                    {
                        lecturers.Add(lecturer);
                    }
                }

                reader.Close();
            }
            
            return lecturers;
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


        //Add a lecturer to the database
        public void Add(Lecturer lecturer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"INSERT INTO lecturer ([lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty])" +
                    $"VALUES (@Id, @RoomNumber, @Firstname, @LastName, @PhoneNumber, @Age, @BarDuty)";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", lecturer.LecturerId);
                command.Parameters.AddWithValue("@RoomNumber", lecturer.RoomNumber);
                command.Parameters.AddWithValue("@FirstName", lecturer.FirstName);
                command.Parameters.AddWithValue("@LastName", lecturer.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", lecturer.PhoneNumber);
                command.Parameters.AddWithValue("@Age", lecturer.Age);
                command.Parameters.AddWithValue("@BarDuty", lecturer.BarDuty);
                //command.Parameters.Add("@BarDuty", System.Data.SqlDbType.Bit).Value = lecturer.BarDuty;

                command.Connection.Open();
                command.ExecuteNonQuery();

            }

        }

        public void Update(Lecturer lecturer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"UPDATE lecturer SET [lecturer_id] = @Id, [room_number] = @RoomNumber, [first_name] = @Firstname, [last_name] = @LastName, [telephone_number] = @PhoneNumber, [age] = @Age, [bar_duty] = @BarDuty WHERE lecturer.[lecturer_id] = @Id;";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", lecturer.LecturerId);
                command.Parameters.AddWithValue("@RoomNumber", lecturer.RoomNumber);
                command.Parameters.AddWithValue("@FirstName", lecturer.FirstName);
                command.Parameters.AddWithValue("@LastName", lecturer.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", lecturer.PhoneNumber);
                command.Parameters.AddWithValue("@Age", lecturer.Age);
                command.Parameters.AddWithValue("@BarDuty", lecturer.BarDuty);


                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Close();
            }

        }

        public void Delete(Lecturer lecturer)
        {

        }

    }
}
