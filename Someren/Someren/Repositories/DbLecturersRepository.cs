using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Someren.Models;
using System.Data;

namespace Someren.Repositories
{
    //One of the criteria in the grading rubric was: "Methods are no longer than 10 lines of code, so I removed *some* redundancy and adhere to that requirement, unless I forgot how to count"
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
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [Deleted] = 0 ORDER BY [last_name]";         //the sql query to execute

            List<Lecturer> lecturers  = ExecuteReadQuery(query, null, null);
            
            return lecturers;
        }

        //This is the literal same as the thing above, but it only gets deleted records! Wooooo
        public List<Lecturer> GetAllDeleted()
        {

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [Deleted] = 1 ORDER BY [last_name]";         //the sql query to execute

            List<Lecturer> lecturers = ExecuteReadQuery(query, null, null);

            return lecturers;
        }

        //returns a list of all lecturers where the LAST NAME CONTAINS THE SEARCHED STRING
        public List<Lecturer> GetFiltered(string lastName)
        {
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName AND [Deleted] = 0 ORDER BY [last_name];";

            List<Lecturer> lecturers = ExecuteReadQuery(query, "@LastName", $"%{lastName}%");

            return lecturers;
        }

        //Does the exact same thing as the get filtered method above, BUT ONLY FOR DELETED RECORDS
        public List<Lecturer> GetFilteredDeleted(string lastName)
        {
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName AND [Deleted] = 1 ORDER BY [last_name];";

            List<Lecturer> lecturers = ExecuteReadQuery(query, "@LastName", $"%{lastName}%");

            return lecturers;
        }


        //Search a lecturer by ID, then return a class with the information of that lecturer, starts with a list so I can use the method I made specifically for Reading records from the database
        public Lecturer? GetById(int lecturerId)
        {
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [lecturer_id] = @Id;";

            List<Lecturer> lecturers = ExecuteReadQuery(query, "@Id", lecturerId.ToString());

            if(lecturers.Count != 0)
            {
                return lecturers[0];
            }
            else
            {
                throw new Exception("Lecturer not found");
            }
        }

        private string? CheckIfAlreadyExists(Lecturer lecturer)
        {
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [lecturer_id] = @Id OR [last_name] = @LastName;";

            List<Lecturer> lecturers = ExecuteValidationQuery(lecturer, query);

            string? idExists = null;
            string? lastNameExists = null;

            foreach (Lecturer l in lecturers)
            {
                if(l.LecturerId == lecturer.LecturerId) 
                { 
                    idExists = "ID already exists, please pick a different ID. ";
                }
                if(l.LastName == lecturer.LastName) 
                { 
                    lastNameExists = "Last name already exists, please pick a different last name.";
                }
            }

            if (idExists != null || lastNameExists != null)
            {
                return $"{idExists}" + $"{lastNameExists}";
            } else
            {
                return null;
            }
        }

        private void CheckIfRoomAvailable(Lecturer lecturer)
        {
            //This gets a list of all available rooms
            string query = "SELECT [room_number] FROM room WHERE [room_type] = 'Lecturer' AND [room_number] LIKE 'A1-%' AND [Deleted] = 0 AND [room_number] NOT IN (SELECT [room_number] FROM [lecturer] WHERE [lecturer_id] != @Id)";

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
            string query = $"INSERT INTO lecturer ([lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty])" +
                                        $"VALUES (@Id, @RoomNumber, @Firstname, @LastName, @PhoneNumber, @Age, @BarDuty);"; ;

            //Check if the last name already exists in the database
            string? checkLecturer = CheckIfAlreadyExists(lecturer);

            CheckIfRoomAvailable(lecturer);

            if (checkLecturer != null)
            {
                throw new Exception(checkLecturer);
            } 
            //If the lecturer is new and the room is available, create a new lecturer in the database
            else 
            {
                ExecuteModificationQuery(lecturer, query);
            }
        }

        //EDIT a lecturer in the database
        public void Update(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET [lecturer_id] = @Id, [room_number] = @RoomNumber, [first_name] = @Firstname, [last_name] = @LastName, [telephone_number] = @PhoneNumber, [age] = @Age, [bar_duty] = @BarDuty WHERE lecturer.[lecturer_id] = @Id;";

            CheckIfRoomAvailable(lecturer);

            ExecuteModificationQuery(lecturer, query);
        }

        //SOFT DELETE a lecturer from the database
        public void Delete(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET Deleted = 1 WHERE lecturer.lecturer_id = @Id;";

            ExecuteModificationQuery(lecturer , query);
        }

        //Restore a deleted lecturer from the database
        public void Restore(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET Deleted = 0 WHERE lecturer.lecturer_id = @Id;";

            ExecuteModificationQuery(lecturer, query);
        }

        //HARD DELETE a lecturer from the database
        public void Erase(Lecturer lecturer)
        {
            string query = $"DELETE FROM lecturer WHERE lecturer.lecturer_id = @Id;";

            ExecuteModificationQuery(lecturer, query);
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

        //This is used for ALL methods that READ from the database (GetAll, GetAllDeleted, GetFiltered, GetFilteredDeleted, GetById, GetByLastName
        private List<Lecturer> ExecuteReadQuery(string query, string? parameterName, string? parameterValue)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {

                SqlCommand command = new SqlCommand(query, connection);

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

        //To execute the Create, Update, and Delete methods, uses a Data Reader because I want to add the error codes **later**
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

        private List<Lecturer> ExecuteValidationQuery(Lecturer lecturer, string query)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {

                SqlCommand command = new SqlCommand(query, connection);

                AddParametersWithValues(command, lecturer);

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
