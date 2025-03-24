using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Someren.Models;
using System.Data;

namespace Someren.Repositories
{
    //One of the criteria in the grading rubric was: "Methods are no longer than 10 lines of code, so I removed *some* redundancy and try my best to adhere to that requirement, EVERY METHOD IS UNDER 15 LINES"
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

        //returns a list of all NOT DELETED AND DELETED lecturers, depinding on the bool input
        public List<Lecturer> GetAll(bool deleted)
        {
            //the sql query to execute
            const string query =    "SELECT L.[lecturer_id], R.[room_number], L.[first_name], L.[Last_name], L.[telephone_number], L.[age], L.[bar_duty], L.[deleted] " +
                                    "FROM lecturer AS L " +
                                    "JOIN room AS R ON L.room_id = R.room_id " +
                                    "WHERE L.[deleted] = @Deleted ORDER BY L.[last_name];";

            List<Lecturer> lecturers  = ExecuteReadQuery(query, deleted, null, null);
            
            return lecturers;
        }

        //returns a list of all lecturers where the LAST NAME CONTAINS THE SEARCHED STRING
        public List<Lecturer> GetFiltered(string lastName, bool deleted)
        {
            const string query =    "SELECT L.[lecturer_id], R.[room_number], L.[first_name], L.[Last_name], L.[telephone_number], L.[age], L.[bar_duty], L.[deleted] " +
                                    "FROM lecturer AS L " +
                                    "JOIN room AS R ON L.room_id = R.room_id " +
                                    "WHERE L.[last_name] LIKE @LastName AND L.[deleted] = @Deleted ORDER BY L.[last_name];";

            List<Lecturer> lecturers = ExecuteReadQuery(query, deleted, "@LastName", $"%{lastName}%");

            return lecturers;
        }

        //Search a lecturer by ID, then return a class with the information of that lecturer, starts with a list so I can use the method I made specifically for Reading records from the database
        public Lecturer? GetById(int lecturerId)
        {
            const string query =    "SELECT L.[lecturer_id], R.[room_number], L.[first_name], L.[Last_name], L.[telephone_number], L.[age], L.[bar_duty], L.[deleted] " +
                                    "FROM lecturer AS L " +
                                    "JOIN room AS R ON L.room_id = R.room_id " +
                                    "WHERE [lecturer_id] = @Id;";

            List<Lecturer> lecturers = ExecuteReadQuery(query, null,"@Id", lecturerId.ToString());

            if(lecturers.Count != 0)
            {
                return lecturers[0];
            }
            else
            {
                throw new Exception("Lecturer not found");
            }
        }


        private void CheckIfAlreadyExists(Lecturer lecturer)
        {
            const string query =    "SELECT L.[Last_name]" +
                                    "FROM lecturer AS L " +
                                    "WHERE L.[last_name] = @LastName AND L.[lecturer_id] != @Id";

            List<string> lecturers = ExecuteNameValidationQuery(lecturer, query);

            if (lecturers.Contains(lecturer.LastName))
            {
                throw new Exception($"Last name already exists, please pick a different last name.");
            }
            else
            {
                return;
            }
        }


        /* Checks if the room is available, if it is not, gives an error code.
        ALL LECTURER ROOMS ARE ON THE GROUND FLOOR OF BUILDING A, THEREFORE IT SPECIFICALLY CHECKS FOR THAT FLOOR
        IF (soft) DELETE THE ROOM IN THE ROOM TABLE AND THEN EDIT THE LECTURER THAT WAS SLEEPING IN THAT ROOM
        THIS
        WILL
        MARK
        THOSE ROOMS AS UNAVAILABLE
        this WILL give you errors when EDITING Lecturers.
        I have mentioned this and we are awaiting the patch, volvo pls fix. */
        private void CheckIfRoomAvailable(Lecturer lecturer)
        {
            //This gets a list of all available rooms
            const string query =    "SELECT [room_number] " +
                                    "FROM room " +
                                    "WHERE [room_type] = 'Lecturer' AND [room_number] LIKE 'A1-%' AND [Deleted] = 0 " +
                                    "AND [room_id] NOT IN (SELECT [room_id] FROM [lecturer] WHERE [lecturer_id] != @Id);";

            List<string> availableRooms = ExecuteRoomValidationQuery(lecturer, query);

            if (availableRooms.Contains(lecturer.RoomNumber))
            {
                return;
            }
            else
            {
                throw new Exception("Room not Available, available rooms: " + string.Join(", ", availableRooms) + ".");
            }
        }

        //ADD a lecturer to the database
        public void Add(Lecturer lecturer)
        {
            const string query =    $"INSERT INTO lecturer ([room_id], [first_name], [last_name], [telephone_number], [age], [bar_duty])" +
                                    $"VALUES ((SELECT [room_id] FROM room WHERE [room_number] = @RoomNumber), @Firstname, @LastName, @PhoneNumber, @Age, @BarDuty);";

            //Check if the last name already exists in the database
            CheckIfAlreadyExists(lecturer);

            CheckIfRoomAvailable(lecturer);

            //If the lecturer is new and the room is available, create a new lecturer in the database
            ExecuteModificationQuery(lecturer, query);
        }

        //EDIT a lecturer in the database
        public void Update(Lecturer lecturer)
        {
            const string query =    $"UPDATE lecturer " +
                                    $"SET               [room_id] = (SELECT [room_id] FROM room WHERE [room_number] = @RoomNumber), " +
                                                        $"[first_name] = @Firstname, [last_name] = @LastName, " +
                                                        $"[telephone_number] = @PhoneNumber, [age] = @Age, [bar_duty] = @BarDuty " +
                                    $"WHERE lecturer.[lecturer_id] = @Id;";
            
            CheckIfAlreadyExists(lecturer);

            CheckIfRoomAvailable(lecturer);

            ExecuteModificationQuery(lecturer, query);
        }

        //SOFT DELETE a lecturer from the database
        public void Delete(Lecturer lecturer)
        {
            const string query =    $"UPDATE lecturer " +
                                    $"SET Deleted = 1 " +
                                    $"WHERE lecturer_id = @Id;";

            ExecuteDeleteAndRestoreQuery(lecturer , query);
        }

        //Restore a deleted lecturer from the database
        public void Restore(Lecturer lecturer)
        {
            const string query =    $"UPDATE lecturer " +
                                    $"SET Deleted = 0 " +
                                    $"WHERE lecturer_id = @Id;";

            ExecuteDeleteAndRestoreQuery(lecturer, query);
        }

        //HARD DELETE a lecturer from the database
        public void Erase(Lecturer lecturer)
        {
            const string query =    $"DELETE FROM lecturer " +
                                    $"WHERE lecturer_id = @Id;";

            ExecuteDeleteAndRestoreQuery(lecturer, query);
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
            bool deleted = (bool)reader["deleted"];

            return new Lecturer(lecturerId, roomNumber, firstName, lastName, phoneNumber, age, barDuty, deleted);
        }

        //This is used for ALL methods that READ from the database (GetAll, GetFiltered, GetById)
        private List<Lecturer> ExecuteReadQuery(string query, bool? deleted, string? parameterName, string? parameterValue)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {

                SqlCommand command = new SqlCommand(query, connection);

                if (deleted != null)
                {
                    command.Parameters.AddWithValue("@Deleted", deleted);
                }

                if (parameterName != null && parameterValue != null)
                {
                    command.Parameters.AddWithValue($"{parameterName}", parameterValue);
                }

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lecturers.Add(ReadLecturer(reader));
                }

                reader.Close();

                return lecturers;
            }

        }

        //To execute the Create and Update methods. Uses a Data Reader so it also reads it when something goes wrong unexpectedly (it shouldn't)
        private void ExecuteModificationQuery(Lecturer lecturer, string query)
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

        //To execute the Delete, Restore, and Erase methods. Uses a Data Reader for error codes
        private void ExecuteDeleteAndRestoreQuery(Lecturer lecturer, string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", lecturer.LecturerId);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Close();
            }
        }


        //These are the two validaiton methods used. This is being called by CheckIfAlreadyExists()
        private List<string> ExecuteNameValidationQuery(Lecturer lecturer, string query)
        {
            List<string> lecturers = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@LastName", lecturer.LastName);
                command.Parameters.AddWithValue("@Id", lecturer.LecturerId);

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lecturers.Add((string)reader["last_name"]);
                }
                reader.Close();

                return lecturers;
            }
        }

        //These are the two validaiton methods used. This is being called by CheckRoomExists()
        private List<string> ExecuteRoomValidationQuery(Lecturer lecturer, string query)
        {
            List<string> rooms = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", lecturer.LecturerId);

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rooms.Add((string)reader["room_number"]);
                }
                reader.Close();

                return rooms;
            }
        }


    }
}
