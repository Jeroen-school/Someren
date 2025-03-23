using Microsoft.Data.SqlClient;
using Someren.Models;
using System.Text.RegularExpressions;
namespace Someren.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly string? _connectionString;

        public RoomRepository(IConfiguration configuration)
        {  
            _connectionString = configuration.GetConnectionString("SomerenDb");
        }

        //validation for room number
        public bool IsValidRoomNumber(string roomNumber)
        {
            string pattern = @"(?i)^[AB]\d-\d{2}$"; // Regex pattern
            return Regex.IsMatch(roomNumber, pattern);
        }

        public bool IsRoomNumberForLecturers(string roomNumber)
        {
            string pattern = @"(?i)^A1-\d{2}$"; // Regex pattern
            return Regex.IsMatch(roomNumber, pattern);
        }
        public bool RoomExists(string roomNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(room_number) FROM room WHERE room_number = @roomNumber AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomNumber", roomNumber);
                command.Parameters.AddWithValue("@deleted", false);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool IsRoomDeleted(string roomNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(room_number) FROM room WHERE room_number = @roomNumber AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomNumber", roomNumber);
                command.Parameters.AddWithValue("@deleted", true);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
        private Room ReadRoom(SqlDataReader reader)
        {
            int roomId = (int)reader["room_id"];
            string roomNumber = (string)reader["room_number"];
            string type = (string)reader["room_type"];
            int size = (int)reader["room_size"];
            bool deleted = (bool)reader["Deleted"];

            return new Room(roomId, roomNumber, type, size, deleted);
        }

        public List<Room> GetAll(int? size)
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room WHERE Deleted = @deleted";

                // If a size is provided, filter by it
                if (size.HasValue)
                {
                    query += " AND room_size = @roomSize";
                }

                query += " ORDER BY room_number";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@deleted", false);

                if (size.HasValue)
                {
                    command.Parameters.AddWithValue("@roomSize", size.Value);
                }

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Room room = ReadRoom(reader);
                    rooms.Add(room);
                }
                reader.Close();
            }
            return rooms;
        }


        public Room? GetById(int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room WHERE room_id = @roomId AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return ReadRoom(reader);
                }
                reader.Close();
                return null;
            }
        }

        public bool Add(Room room, out string errorMessage)
        {
            errorMessage = "";

            // Validate Room Number Format
            if (!IsValidRoomNumber(room.RoomNumber))
            {
                errorMessage = "Invalid room number format. Format must be A1-02 or B2-05.";
                return false;
            }

            // Check if Room Already Exists
            if (RoomExists(room.RoomNumber))
            {
                errorMessage = "Room already exists.";
                return false;
            }

            // Return error message if room number should be for lecturers
            if (room.Type == "Student" && IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Students can not sleep in building A ground floor (A1).";
                return false;
            }

            // Return error message if room number should be for students
            if (room.Type == "Lecturer" && !IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Lecturers can only sleep in building A ground floor (A1).";
                return false;
            }

            // If validation passes, insert the room
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query;
                if (IsRoomDeleted(room.RoomNumber))
                {
                    query = "UPDATE room SET room_number = @roomNumber, room_size = @roomSize, room_type = @roomType, Deleted = @deleted WHERE room_number = @roomNumber";
                }
                else
                {
                    query = "INSERT INTO room (room_number, room_size, room_type, Deleted) " +
                                   "VALUES (@roomNumber, @roomSize, @roomType, @deleted)";
                }
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber.ToUpper());
                command.Parameters.AddWithValue("@roomSize", room.Size);
                command.Parameters.AddWithValue("@roomType", room.Type);
                command.Parameters.AddWithValue("@deleted", false);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return true;
        }
        public bool Update(Room room, out string errorMessage)
        {
            errorMessage = "";

            // Validate Room Number Format
            if (!IsValidRoomNumber(room.RoomNumber))
            {
                errorMessage = "Invalid room number format. Format must be A1-02 or B2-05.";
                return false;
            }

            Room? savedRoom = GetById(room.RoomId);

            // Check if Room Already Exists
            if (savedRoom != null && savedRoom.RoomNumber != room.RoomNumber && RoomExists(room.RoomNumber))
            {
                errorMessage = "Room already exists.";
                return false;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                string query = "UPDATE room SET room_number = @roomNumber, room_size = @roomSize, room_type = @roomType WHERE room_id = @roomId AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber);
                command.Parameters.AddWithValue("@roomId", room.RoomId);
                command.Parameters.AddWithValue("@roomSize", room.Size);
                command.Parameters.AddWithValue("@roomType", room.Type);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            return true;
        }

        public void Delete(int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE room SET Deleted = @deleted WHERE room_id = @roomId";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", true);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
